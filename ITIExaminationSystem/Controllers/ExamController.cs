using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ITIExaminationSystem.Models;
using ITIExaminationSystem.Models.ModelView;
using ITIExaminationSystem.Models.DTOs.Exam;

namespace ITIExamination.Controllers
{
    public class ExamController : Controller
    {
        private readonly ExaminationSystemContext context;

        public ExamController(ExaminationSystemContext _context)
        {
            context = _context;
        }

        // =========================================================
        // START EXAM (SQL CONTROLS EVERYTHING)
        // =========================================================
        [HttpGet]
        public IActionResult Start(int examId, int studentId)
        {
            // 1️⃣ Validate exam access (student + assignment + time)
            var exam = context.ExamAccessDtos
                .FromSqlRaw(
                    "EXEC dbo.sp_Exam_ValidateAccess @ExamId, @StudentId",
                    new SqlParameter("@ExamId", examId),
                    new SqlParameter("@StudentId", studentId)
                )
                .AsEnumerable()
                .FirstOrDefault();

            if (exam == null)
            {
                TempData["ErrorMessage"] = "You cannot access this exam at this time.";
                return RedirectToAction("ExamNotStarted");
            }

            // 2️⃣ Load random questions
            var rawQuestions = context.QuestionChoiceDtos
                .FromSqlRaw(
                    "EXEC dbo.sp_Exam_GetQuestions @CourseId, @McqCount, @TfCount",
                    new SqlParameter("@CourseId", exam.Course_Id),
                    new SqlParameter("@McqCount", exam.McqCount ?? 0),
                    new SqlParameter("@TfCount", exam.TrueFalseCount ?? 0)
                )
                .ToList();

            if (!rawQuestions.Any())
                return BadRequest("No questions found.");

            // 3️⃣ Group questions with choices
            var questions = rawQuestions
                .GroupBy(q => new { q.Question_Id, q.Question_Text, q.Question_Type })
                .Select(g => new ExamQuestionViewModel
                {
                    QuestionId = g.Key.Question_Id,
                    Text = g.Key.Question_Text,
                    Type = g.Key.Question_Type,
                    Choices = g
                        .Where(x => x.Choice_Id.HasValue)
                        .Select(c => new ExamChoiceViewModel
                        {
                            ChoiceId = c.Choice_Id!.Value,
                            Text = c.Choice_Text!
                        })
                        .ToList()
                })
                .ToList();

            // 4️⃣ Build ViewModel
            var vm = new TakeExamViewModel
            {
                id = exam.Exam_Id,
                Name = "Exam",
                Duration = exam.Duration ?? 60,
                TotalScore = exam.Full_Marks,
                QuestionList = questions
            };

            ViewBag.StudentId = studentId;
            ViewBag.ExamId = examId;

            return View("Exam", vm);
        }

        // =========================================================
        // SUBMIT EXAM (SQL HANDLES EVERYTHING)
        // =========================================================
        [HttpPost]
        public IActionResult SubmitExam(SubmitExamViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.AnswersJson))
                return BadRequest("No answers submitted.");

            context.Database.ExecuteSqlRaw(
                "EXEC dbo.sp_Exam_Submit @Exam, @Student, @Json",
                new SqlParameter("@Exam", model.ExamId),
                new SqlParameter("@Student", model.StudentId),
                new SqlParameter("@Json", model.AnswersJson)
            );

            return RedirectToAction("StudentHome", "Student", new { id = model.StudentId });
        }

        // =========================================================
        // SUPPORT VIEWS
        // =========================================================
        public IActionResult ExamExpired() => View();

        public IActionResult ExamNotStarted() => View();

        public IActionResult Finished() => View();
    }
}
