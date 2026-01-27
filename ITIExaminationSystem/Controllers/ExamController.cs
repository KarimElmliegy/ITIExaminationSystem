using System;
using System.Text.Json;
using ITIExaminationSystem.Models;
using ITIExaminationSystem.Models.ModelView;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ITIExamination.Controllers
{
    public class ExamController : Controller
    {
        private readonly ExaminationSystemContext context;

        public ExamController(ExaminationSystemContext _context)
        {
            context = _context;
        }
        [HttpGet]
        public IActionResult Start(int examId, int studentId)
        {
            Console.WriteLine("=================================================");
            Console.WriteLine($"🚀 START EXAM DEBUGGING: ExamId={examId}, StudentId={studentId}");

            // =========================================================
            // 1️⃣ VALIDATE STUDENT
            // =========================================================
            var student = context.Students
                .Include(s => s.User)
                .FirstOrDefault(s => s.StudentId == studentId);

            if (student == null)
            {
                Console.WriteLine("❌ ERROR: Student not found.");
                return Unauthorized("Student not found.");
            }

            Console.WriteLine($"✅ Student Found: ID={student.StudentId}, Branch={student.BranchId}, Track={student.TrackId}");

            // =========================================================
            // 2️⃣ VALIDATE EXAM ASSIGNMENT (TRACK + BRANCH)
            // =========================================================
            var assign = context.Assigns
                .Include(a => a.Exam)
                    .ThenInclude(e => e.Course)
                .FirstOrDefault(a =>
                    a.ExamId == examId &&
                    a.BranchId == student.BranchId &&
                    a.TrackId == student.TrackId
                );

            if (assign == null)
            {
                Console.WriteLine("❌ ERROR: Exam not assigned to this student's track/branch.");
                return BadRequest("This exam is not assigned to your track.");
            }

            var exam = assign.Exam;

            // =========================================================
            // 3️⃣ VALIDATE EXAM TIME
            // =========================================================
            if (exam.Date.HasValue && exam.Time.HasValue && exam.Duration.HasValue)
            {
                DateTime examStart = exam.Date.Value.ToDateTime(exam.Time.Value);
                DateTime examEnd = examStart.AddMinutes(exam.Duration.Value);

                if (DateTime.Now > examEnd)
                {
                    Console.WriteLine("❌ ERROR: Exam time ended.");
                    TempData["ErrorMessage"] = "This exam has already ended.";
                    return RedirectToAction("ExamExpired");
                }

                if (DateTime.Now < examStart)
                {
                    Console.WriteLine("❌ ERROR: Exam has not started.");
                    TempData["ErrorMessage"] = $"This exam will start at {examStart:yyyy-MM-dd HH:mm}.";
                    return RedirectToAction("ExamNotStarted");
                }
            }

            Console.WriteLine($"✅ Exam Valid. CourseId={exam.CourseId}, Duration={exam.Duration}");

            // =========================================================
            // 4️⃣ FETCH QUESTIONS
            // =========================================================
            int mcqCount = exam.McqCount ?? 0;
            int tfCount = exam.TrueFalseCount ?? 0;

            // ✅ MCQs
            var mcqs = context.Questions
                .Include(q => q.Choices)
                .Where(q =>
                    q.CourseId == exam.CourseId &&
                    q.QuestionType == "MCQ" &&
                    q.Choices.Count >= 2
                )
                .OrderBy(q => Guid.NewGuid())
                .Take(mcqCount)
                .ToList();

            // ✅ TRUE / FALSE
            var tfs = context.Questions
                .Where(q =>
                    q.CourseId == exam.CourseId &&
                    q.QuestionType == "TF"
                )
                .OrderBy(q => Guid.NewGuid())
                .Take(tfCount)
                .ToList();

            var allQuestions = mcqs
                .Concat(tfs)
                .OrderBy(q => Guid.NewGuid())
                .ToList();

            if (!allQuestions.Any())
            {
                Console.WriteLine("🔴 CRITICAL: No questions found.");
                return BadRequest("No questions were found for this exam.");
            }

            Console.WriteLine($"🏁 FINAL: {allQuestions.Count} questions loaded.");

            // =========================================================
            // 5️⃣ BUILD VIEW MODEL
            // =========================================================
            var examData = new TakeExamViewModel
            {
                id = exam.ExamId,
                Name = exam.Course.CourseName,
                Duration = exam.Duration ?? 60,
                TotalScore = exam.FullMarks,
                QuestionList = allQuestions.Select(q => new ExamQuestionViewModel
                {
                    QuestionId = q.QuestionId,
                    Text = q.QuestionText,
                    Type = q.QuestionType,
                    Choices = q.QuestionType == "MCQ"
                        ? q.Choices.Select(c => new ExamChoiceViewModel
                        {
                            ChoiceId = c.ChoiceId,
                            Text = c.ChoiceText
                        }).ToList()
                        : new List<ExamChoiceViewModel>()
                }).ToList()
            };

            ViewBag.StudentId = student.StudentId;
            ViewBag.ExamId = examId;

            Console.WriteLine("=================================================");

            return View("Exam", examData);
        }

        [HttpPost]
        public IActionResult SubmitExam(SubmitExamViewModel model)
        {
            // 1. Validate Exam
            var exam = context.Exams.FirstOrDefault(e => e.ExamId == model.ExamId);
            if (exam == null) return BadRequest("Invalid exam.");

            // 2. Validate Expiration
            if (exam.Date.HasValue && exam.Time.HasValue && exam.Duration.HasValue)
            {
                var examEnd = exam.Date.Value.ToDateTime(exam.Time.Value).AddMinutes(exam.Duration.Value);
                // Add 5 minutes buffer
                if (DateTime.Now > examEnd.AddMinutes(5))
                {
                    TempData["ErrorMessage"] = "Exam time has ended.";
                    return RedirectToAction("ExamExpired");
                }
            }

            // ============================================================
            // 3. SAVE ANSWERS (FIXED LOGIC)
            // ============================================================
            if (!string.IsNullOrEmpty(model.AnswersJson))
            {
                try
                {
                    // FIX 1: Use Case-Insensitive Options to guarantee matching
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var submittedList = JsonSerializer.Deserialize<List<SubmittedAnswer>>(model.AnswersJson, options);

                    if (submittedList != null && submittedList.Count > 0)
                    {
                        foreach (var item in submittedList)
                        {
                            // Create the Answer
                            var dbAnswer = new Answer
                            {
                                ExamId = model.ExamId,
                                StudentId = model.StudentId,
                                QuestionId = item.QuestionId,
                                TfSelected = item.TrueFalseAnswer,
                                ScoredMarks = 0
                            };

                            // Add Answer to generate ID
                            context.Answers.Add(dbAnswer);
                            context.SaveChanges();

                            // Handle Choices (MCQ)
                            if (item.ChoiceId.HasValue)
                            {
                                var choice = context.Choices.Find(item.ChoiceId.Value);
                                if (choice != null)
                                {
                                    // FIX 2: Prevent NullReferenceException
                                    if (dbAnswer.Choices == null)
                                    {
                                        dbAnswer.Choices = new List<Choice>();
                                    }

                                    dbAnswer.Choices.Add(choice);
                                    context.SaveChanges();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // IMPORTANT: If you are debugging, put a breakpoint here to see the error!
                    Console.WriteLine("❌ CRITICAL ERROR SAVING ANSWERS: " + ex.Message);
                    // Optional: You might want to return an error view here instead of redirecting silently
                }
            }

            TempData["SuccessMessage"] = "Exam submitted successfully!";

            // 4. Redirect to Student Home
            var student = context.Students.FirstOrDefault(s => s.StudentId == model.StudentId);

            // Fetch the answers we just saved, including related data
            var results = context.Answers
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Question)
                .Include(a => a.Choices) // The choices the student selected
                .Where(a => a.ExamId == model.ExamId && a.StudentId == model.StudentId)
                .ToList();

            Console.WriteLine("\n--- EXAM SUBMISSION SUMMARY ---");
            Console.WriteLine($"Student: {results.FirstOrDefault()?.Student?.User?.FName} (ID: {model.StudentId})");
            Console.WriteLine($"Exam ID: {model.ExamId}");
            Console.WriteLine("--------------------------------");

            foreach (var ans in results)
            {
                Console.WriteLine($"Question: {ans.Question.QuestionText}");

                if (ans.Question.QuestionType == "MCQ")
                {
                    // Get the text of the selected choice(s)
                    var selectedChoice = ans.Choices.FirstOrDefault()?.ChoiceText ?? "No Choice Selected";
                    Console.WriteLine($" Student Selected (MCQ): {selectedChoice}");
                }
                else // True/False
                {
                    Console.WriteLine($" Student Selected (T/F): {ans.TfSelected}");
                }
                Console.WriteLine("--------------------------------");
            }
            if (student != null)
            {
                // Redirect using the UserID (Required for your StudentHome logic)
                return RedirectToAction("StudentHome", "Student", new { id = student.UserId });
            }

            return RedirectToAction("Index", "Home");
        }
        public IActionResult ExamExpired()
        {
            return View();
        }

        // New action for exam not started
        public IActionResult ExamNotStarted()
        {
            return View();
        }

        public IActionResult Finished()
        {
            return View();
        }

        // DTO for raw SQL
        public class QuestionChoiceDto
        {
            public int Question_Id { get; set; }
            public string Question_Text { get; set; }
            public string Question_Type { get; set; }
            public int Choice_Id { get; set; }
            public string Choice_Text { get; set; }
            public int IsCorrect { get; set; }
        }
    
    }
}