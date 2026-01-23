using ITIExaminationSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Formats.Asn1.AsnWriter;

namespace ITIExamination.Controllers
{
    public class StudentController : Controller
    {
        ExaminationSystemContext context = new ExaminationSystemContext();

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult StudentHome(int id)
        {
            // 1. Get the Student with Track info
            var student = context.Students
                .Include(s => s.User)
                .Include(s => s.Branch)
                .Include(s => s.Track)
                .FirstOrDefault(s => s.UserId == id);

            if (student == null) return NotFound();

            var today = DateOnly.FromDateTime(DateTime.Now);

            // 2. Fetch ALL Courses in the Student's TRACK
            // We filter by 'Teaches' table to find courses assigned to this Track
            var trackCourses = context.Tracks
                                      .Where(t => t.TrackId == student.TrackId)
                                      .SelectMany(t => t.Courses)
                                      .Include(c => c.Topics)
                                      .Include(c => c.Exams).ThenInclude(e => e.Answers)
                                      .Include(c => c.Exams).ThenInclude(e => e.Questions)
                                      .Include(c => c.Teaches)
                                      .ThenInclude(t => t.Instructor)
                                      .ThenInclude(i => i.User)
                                      .ToList();


            // 3. Map to ViewModel
            var result = trackCourses.Select(x => new CourseViewModel
            {
                Id = x.CourseId,
                Title = x.CourseName,
                Code = x.CourseId, // Use x.Code if you have that property

                // Generate a description (since you likely don't have this column yet)
                Description = $"A complete module on {x.CourseName} designed for the {student.Track.TrackName} track.",

                Modules = x.Topics.Count,
                Exams = x.Exams.Count,
                Topics = x.Topics.Select(t => t.TopicName).ToList(),

                // Status Logic
                Status = GetCourseStatus(x, student.StudentId),

                // Check if THIS student has completed the exams
                Completed = x.Exams.Count(e => e.Answers.Any(a => a.StudentId == student.StudentId)),

                // Get the Instructor assigned to THIS Track for THIS Course
                Instructor = x.Teaches
                    .Where(t => t.TrackId == student.TrackId)
                    .Select(t => t.Instructor.User.UserName)
                    .FirstOrDefault() ?? "TBA",

                // Map Exams
                ExamList = x.Exams.Select(e => new ExamViewModel
                {
                    id = e.ExamId,
                    Type = "Exam",

                    // Fix: Convert DateOnly to String for safe JSON serialization
                    Date = e.Date.HasValue ? e.Date.Value.ToString("MMMM dd, yyyy") : "TBA",

                    Duration = e.Duration,
                    Questions = e.Questions.Count,

                    // Logic: Exam is available if Date is <= Today AND Today <= Date + 7 days
                    Available = e.Date.HasValue &&
                                e.Date.Value <= today &&
                                e.Date.Value.AddDays(7) >= today,

                    // Score for THIS student
                    Score = e.Answers
                        .Where(a => a.StudentId == student.StudentId)
                        .Sum(a => a.ScoredMarks),

                    IsCompleted = e.Answers.Any(a => a.StudentId == student.StudentId)
                }).ToList()
            }).ToList();

            // 4. Update ViewBags
            ViewBag.Track = student.Track;
            ViewBag.Branch = student.Branch;
            ViewBag.TrackCourses = result.Count; // This will now show the total courses in the track
            ViewBag.TotalExams = result.Sum(c => c.Exams);
            ViewBag.CompletedExams = result.Sum(c => c.Completed);
            ViewBag.TotalModules = result.Sum(c => c.Modules);

            // Pass to View
            ViewBag.Courses = result;

            return View("~/Views/Student/StudentHome.cshtml", student);
        }
        private string GetCourseStatus(Course course, int studentId)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            var exams = course.Exams;

            if (!exams.Any())
                return "UPCOMING";

            // Completed: student has answers for all exams
            var completedExamIds = exams
                .SelectMany(e => e.Answers)
                .Where(a => a.StudentId == studentId)
                .Select(a => a.ExamId)
                .Distinct()
                .ToList();

            if (completedExamIds.Count == exams.Count)
                return "COMPLETED";

            // Active: any exam currently available
            var hasActiveExam = exams.Any(e =>
                e.Date.HasValue &&
                e.Date.Value <= today &&
                e.Date.Value.AddDays(7) >= today);

            if (hasActiveExam)
                return "ACTIVE";

            return "UPCOMING";
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Student student)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    return RedirectToAction("Index");
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes.");
                }
            }
            return RedirectToAction("Index");
        }
    }

    // ViewModels
    public class CourseViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Code { get; set; }
        public string Instructor { get; set; }
        public int Modules { get; set; }
        public int Exams { get; set; }
        public int Completed { get; set; }
        public string Status { get; set; }
        public List<string> Topics { get; set; }
        public string Description { get; set; }   // ✅ ADD THIS
        public List<ExamViewModel> ExamList { get; set; }
    }

    public class ExamViewModel
    {
        public int id { get; set; }
        public string Type { get; set; }
        public string Date { get; set; } // Must be string
        public double? Duration { get; set; }
        public int Questions { get; set; }
        public bool Available { get; set; }
        public int? Score { get; set; }
        public bool IsCompleted { get; set; }
    }
}