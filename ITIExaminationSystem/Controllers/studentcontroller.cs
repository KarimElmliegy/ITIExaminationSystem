using ITIExaminationSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITIExamination.Controllers
{
    public class StudentController : Controller
    {
        private readonly ExaminationSystemContext context = new ExaminationSystemContext();

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult StudentHome(int id)
        {
            // 1️⃣ Get student by USER ID
            var student = context.Students
                .Include(s => s.User)
                .Include(s => s.Branch)
                .Include(s => s.Track)
                // We don't rely on s.Courses here anymore for the list, 
                // but we keep the Include just in case you use it elsewhere.
                .Include(s => s.Courses)
                .FirstOrDefault(s => s.User.UserId == id);

            if (student == null)
                return NotFound();

            var now = DateTime.Now;
            var today = DateOnly.FromDateTime(now);

            // =========================================================
            // ✅ FIX: Fetch ALL courses in the student's TRACK
            // =========================================================
            var trackCourses = context.Courses
                .Where(c => c.Tracks.Any(t => t.TrackId == student.TrackId)) // Filter by Track
                .Include(c => c.Topics)
                .Include(c => c.Exams)
                    .ThenInclude(e => e.Answers)
                .Include(c => c.Teaches)
                    .ThenInclude(t => t.Instructor)
                        .ThenInclude(i => i.User)
                .ToList();

            // 3️⃣ Map to ViewModel with Expiration & Timer Logic
            // Notice we iterate over 'trackCourses' now, not 'student.Courses'
            var result = trackCourses.Select(x => new CourseViewModel
            {
                Id = x.CourseId,
                Title = x.CourseName,
                Code = x.CourseId,
                Description = $"A complete module on {x.CourseName} designed for the {student.Track.TrackName} track.",
                Modules = x.Topics.Count,
                Exams = x.Exams.Count,
                Topics = x.Topics.Select(t => t.TopicName).ToList(),

                // Status
                Status = GetCourseStatus(x, student.StudentId),

                // Completed exams count
                Completed = x.Exams.Count(e =>
                    e.Answers.Any(a => a.StudentId == student.StudentId)
                ),

                // Instructor
                Instructor = x.Teaches
                    .Where(t => t.TrackId == student.TrackId)
                    .Select(t => t.Instructor.User.UserName)
                    .FirstOrDefault() ?? "TBA",

                // Exam List
                ExamList = x.Exams.Select(e =>
                {
                    // 🕒 CALCULATION LOGIC
                    bool isExpired = false;
                    DateTime? examStart = null;

                    if (e.Date.HasValue && e.Time.HasValue)
                    {
                        examStart = e.Date.Value.ToDateTime(e.Time.Value);

                        if (e.Duration.HasValue)
                        {
                            var examEnd = examStart.Value.AddMinutes(e.Duration.Value);
                            if (now > examEnd)
                            {
                                isExpired = true;
                            }
                        }
                    }

                    return new StudentExamSummaryViewModel
                    {
                        id = e.ExamId,
                        Name = $"{x.CourseName} Exam",
                        Type = "Exam",
                        Date = e.Date.HasValue ? e.Date.Value.ToString("MMMM dd, yyyy") : "TBA",
                        StartTime = e.Time.HasValue ? DateTime.Today.Add(e.Time.Value.ToTimeSpan()).ToString("h:mm tt") : "TBA",
                        FullStartDate = examStart,
                        Duration = e.Duration,
                        QuestionCount = e.Questions.Count,
                        IsExpired = isExpired,

                        // Available Logic
                        Available = !isExpired &&
                                    examStart.HasValue &&
                                    now >= examStart.Value &&
                                    e.Date.Value.AddDays(7) >= today,

                        Score = e.Answers
                            .Where(a => a.StudentId == student.StudentId)
                            .Sum(a => a.ScoredMarks),
                        TotalScore = e.FullMarks,
                        IsCompleted = e.Answers.Any(a => a.StudentId == student.StudentId)
                    };
                }).ToList()

            }).ToList();

            // 4️⃣ Dashboard values
            ViewBag.Track = student.Track;
            ViewBag.Branch = student.Branch;
            ViewBag.TrackCourses = result.Count;
            ViewBag.TotalExams = result.Sum(c => c.Exams);
            ViewBag.CompletedExams = result.Sum(c => c.Completed);
            ViewBag.IntakeNumber = student.IntakeNumber;
            ViewBag.Courses = result;

            return View("StudentHome", student);
        }
        // ==========================================
        // COURSE STATUS LOGIC
        // ==========================================
        private string GetCourseStatus(Course course, int studentId)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var exams = course.Exams;

            if (!exams.Any())
                return "UPCOMING";

            var completedExamIds = exams
                .SelectMany(e => e.Answers)
                .Where(a => a.StudentId == studentId)
                .Select(a => a.ExamId)
                .Distinct()
                .ToList();

            if (completedExamIds.Count == exams.Count)
                return "COMPLETED";

            var hasActiveExam = exams.Any(e =>
                e.Date.HasValue &&
                e.Date.Value <= today &&
                e.Date.Value.AddDays(7) >= today);

            if (hasActiveExam)
                return "ACTIVE";

            return "UPCOMING";
        }
    }

    // ==========================================
    // VIEW MODELS
    // ==========================================
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
        public string Description { get; set; }
        public List<StudentExamSummaryViewModel> ExamList { get; set; }
    }

    public class StudentExamSummaryViewModel
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Date { get; set; }
        public string StartTime { get; set; }
        public DateTime? FullStartDate { get; set; } // ✅ ADD THIS for the countdown
        public bool IsExpired { get; set; } 
        public double? Duration { get; set; }
        public int QuestionCount { get; set; }
        public bool Available { get; set; }
        public int? Score { get; set; }
        public int? TotalScore { get; set; }
        public bool IsCompleted { get; set; }
    }
}
