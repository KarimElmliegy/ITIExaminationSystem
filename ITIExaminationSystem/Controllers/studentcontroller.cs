using ITIExaminationSystem.Models;
using ITIExaminationSystem.Models.ModelView;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ITIExaminationSystem.Models.DTOs.Students;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ITIExamination.Controllers
{
    public class StudentController : Controller
    {
        private readonly ExaminationSystemContext context;

        public StudentController(ExaminationSystemContext context)
        {
            this.context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult StudentHome(int id)
        {
            // =========================
            // 1️⃣ Student Profile
            // =========================
            var student = context.StudentProfileDtos
                .FromSqlRaw(
                    "EXEC sp_Student_GetProfile @UserId",
                    new SqlParameter("@UserId", id)
                )
                .AsEnumerable()
                .FirstOrDefault();

            if (student == null)
                return NotFound();

            // =========================
            // 2️⃣ Exams (Grouped by Course)
            // =========================
            var exams = context.CourseExamDtos
               .FromSqlRaw(
                    "EXEC sp_Student_GetTrackCoursesWithExams @StudentId",
                    new SqlParameter("@StudentId", student.Student_Id)
                )
                .AsEnumerable()
                .ToList();

            var courseGroups = exams
                .GroupBy(e => new { e.Course_Id, e.Course_Name });

            var courseViewModels = new List<CourseViewModel>();

            foreach (var group in courseGroups)
            {
                var courseId = group.Key.Course_Id;
                var courseName = group.Key.Course_Name;

                // =========================
                // 3️⃣ Instructor
                // =========================
                var instructor = context.CourseInstructorDtos
                    .FromSqlRaw(
                        "EXEC sp_Student_GetCourseInstructor @CourseId, @TrackId",
                        new SqlParameter("@CourseId", courseId),
                        new SqlParameter("@TrackId", student.Track_Id)
                    )
                    .AsEnumerable()
                    .FirstOrDefault();

                // =========================
                // 4️⃣ Exams Mapping
                // =========================
                var examSummaries = group.Select(e =>
                {
                    DateTime? fullStartDate = null;
                    bool isExpired = false;

                    if (e.Date.HasValue && e.Time.HasValue)
                    {
                        var date = e.Date.Value;
                        var time = e.Time.Value;
                        fullStartDate = new DateTime(
                            date.Year,
                            date.Month,
                            date.Day,
                            time.Hour,
                            time.Minute,
                            time.Second
                        );

                        if (e.Duration.HasValue &&
                            DateTime.Now > fullStartDate.Value.AddMinutes(e.Duration.Value))
                        {
                            isExpired = true;
                        }
                    }

                    return new StudentExamSummaryViewModel
                    {
                        id = e.Exam_Id ?? 0,
                        Name = $"{courseName} Exam",
                        Type = "Exam",
                        Date = e.Date?.ToString("MMMM dd, yyyy") ?? "TBA",
                        StartTime = e.Time.HasValue
                            ? DateTime.Today.Add(e.Time.Value.ToTimeSpan()).ToString("h:mm tt")
                            : "TBA",
                        FullStartDate = fullStartDate,
                        Duration = e.Duration,
                        QuestionCount = e.QuestionCount??0,
                        Score = e.StudentScore,
                        TotalScore = e.Full_Marks,
                        IsCompleted = e.IsCompleted,
                        IsExpired = isExpired,
                        Available = fullStartDate.HasValue && !isExpired
                    };
                }).ToList();

                // =========================
                // 5️⃣ Topics for this course   ← NEW
                // =========================
                var topics = context.CourseTopicDtos
                    .FromSqlRaw(
                        "EXEC sp_Student_GetCourseTopics @CourseId",
                        new SqlParameter("@CourseId", courseId)
                    )
                    .AsEnumerable()
                    .Select(t => t.TopicName)
                    .ToList();

                // =========================
                // 6️⃣ Course ViewModel
                // =========================
                courseViewModels.Add(new CourseViewModel
                {
                    Id = courseId,
                    Code = courseId,
                    Title = courseName,
                    Instructor = instructor?.InstructorName ?? "TBA",
                    Modules = topics.Count,         // ← real count from DB
                    Exams = examSummaries.Count,
                    Completed = examSummaries.Count(e => e.IsCompleted),
                    Status = examSummaries.All(e => e.IsCompleted)
                        ? "COMPLETED"
                        : examSummaries.Any(e => e.Available)
                            ? "ACTIVE"
                            : "UPCOMING",
                    Topics = topics,                // ← real topics from DB
                    Description = $"A complete module on {courseName} for {student.Track_Name}",
                    ExamList = examSummaries
                });
            }

            // =========================
            // 7️⃣ Dashboard ViewBag
            // =========================
            ViewBag.Track = student.Track_Name;
            ViewBag.Branch = student.Branch_Name;
            ViewBag.TrackCourses = courseViewModels.Count;
            ViewBag.TotalExams = courseViewModels.Sum(c => c.Exams);
            ViewBag.CompletedExams = courseViewModels.Sum(c => c.Completed);
            ViewBag.IntakeNumber = student.Intake_Number;
            ViewBag.Courses = courseViewModels;

            return View("StudentHome", student);
        }
    }
}