using Microsoft.AspNetCore.Mvc;
using ITIExaminationSystem.Models;
using ITIExaminationSystem.Controllers; 
using ITIExaminationSystem.Models.ModelView;
using Microsoft.EntityFrameworkCore;

namespace ITIExaminationSystem.Controllers
{
    public class InstructorController : Controller
    {
        private readonly ILogger<InstructorController> _logger;
        private readonly ExaminationSystemContext _context;

        public InstructorController(
            ILogger<InstructorController> logger,
            ExaminationSystemContext context)
        {
            _logger = logger;
            _context = context;
        }

        // =========================
        // BASIC PAGES
        // =========================

        public IActionResult Index() => View();

        public IActionResult LogOut()
        {
            return RedirectToAction("LoginPage", "Home");
        }

        // =========================
        // STUDENTS
        // =========================

        public IActionResult insDiplayStudents()
        {
            var students = _context.Students
                .Include(s => s.User)
                .ToList();

            ViewBag.Tracks = _context.Tracks.ToList();
            ViewBag.instractors = _context.Instructors.Count();
            ViewBag.StudentCount = _context.Students.Count();
            ViewBag.CourseCount = _context.Courses.Count();

            return View("insDiplayStudents", students);
        }

        public IActionResult FilterByIntake(int? intake)
        {
            int selectedIntake = intake ?? 46;
            ViewBag.SelectedIntake = selectedIntake;

            var students = _context.Students
                .Include(s => s.User)
                .Where(s => s.IntakeNumber == selectedIntake)
                .ToList();

            ViewBag.Tracks = _context.Tracks.ToList();
            return View("insDiplayStudents", students);
        }

        [HttpPost]
        public IActionResult AddStudent(StudentAdd s)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("insDiplayStudents");

            var user = new User
            {
                UserEmail = s.Email,
                UserName = s.FullName,
                UserPassword = s.Password,
                Role = "Student"
            };

            var student = new Student
            {
                TrackId = s.Track,
                BranchId = s.Branch,
                IntakeNumber = s.Intake,
                User = user
            };

            _context.Students.Add(student);
            _context.SaveChanges();

            return RedirectToAction("insDiplayStudents");
        }

        [HttpPost]
        public IActionResult UpdateStudent(StudentUpdate s)
        {
            var student = _context.Students
                .Include(st => st.User)
                .FirstOrDefault(st => st.User.UserId == s.UserId);

            if (student == null)
                return NotFound();

            student.BranchId = s.Branch;
            student.TrackId = s.Track;
            student.IntakeNumber = s.Intake;

            student.User.UserEmail = s.Email;
            student.User.UserName = s.FullName;

            _context.SaveChanges();
            return RedirectToAction("insDiplayStudents");
        }

        [HttpPost]
        public IActionResult DeleteStudent(int userId)
        {
            var student = _context.Students
                .Include(s => s.User)
                .Include(s => s.Courses)
                .Include(s => s.Answers)
                .Include(s => s.Solves)
                .FirstOrDefault(s => s.UserId == userId);

            if (student == null)
                return NotFound();

            student.Courses.Clear();
            _context.Answers.RemoveRange(student.Answers);
            _context.Solves.RemoveRange(student.Solves);

            _context.Students.Remove(student);
            _context.Users.Remove(student.User);

            _context.SaveChanges();
            return Ok();
        }

        // =========================
        // COURSES
        // =========================

        public IActionResult Courses()
        {
            var courses = _context.Courses
                .Include(c => c.Teaches)
                    .ThenInclude(t => t.Instructor)
                        .ThenInclude(i => i.User)
                .ToList();

            return View("Courses", courses);
        }

        [HttpPost]
        public IActionResult AddCourse([FromBody] CourseDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (model.CourseId.HasValue)
            {
                var course = _context.Courses.Find(model.CourseId.Value);
                if (course == null) return NotFound();

                course.CourseName = model.CourseName;
                course.Duration = model.Duration;
                _context.SaveChanges();
                return Ok();
            }

            var courseNew = new Course
            {
                CourseName = model.CourseName,
                Duration = model.Duration
            };

            _context.Courses.Add(courseNew);
            _context.SaveChanges();
            return Ok();
        }

        [HttpPost]
        public IActionResult EditCourse([FromBody] EditCourseDto model)
        {
            var course = _context.Courses.Find(model.CourseId);
            if (course == null) return NotFound();

            course.CourseName = model.CourseName;
            course.Duration = model.Duration;
            _context.SaveChanges();

            return Ok();
        }

        [HttpPost]
        public IActionResult DeleteCourse(int courseId)
        {
            var course = _context.Courses
                .Include(c => c.Exams)
                .ThenInclude(e => e.Assigns)
                .FirstOrDefault(c => c.CourseId == courseId);

            if (course == null)
                return NotFound();

            foreach (var exam in course.Exams)
                _context.Assigns.RemoveRange(exam.Assigns);

            _context.Exams.RemoveRange(course.Exams);
            _context.Courses.Remove(course);
            _context.SaveChanges();

            return Ok();
        }

        // =========================
        // EXAMS
        // =========================

        public async Task<IActionResult> Exams()
        {
            var exams = await _context.Exams
                .Include(e => e.Course)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            ViewBag.Courses = await _context.Courses.ToListAsync();
            ViewBag.Branches = await _context.Branches.ToListAsync();
            ViewBag.Tracks = await _context.Tracks.ToListAsync();

            return View("CreateExam", exams);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateExam(CreateExamModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Exams");

            var exam = new Exam
            {
                CourseId = model.CourseId,
                Date = DateOnly.Parse(model.Date),
                Time = TimeOnly.Parse(model.Time),
                Duration = (double)model.Duration,
                FullMarks = model.FullMarks,

                // ✅ IMPORTANT
                McqCount = model.McqCount,
                TrueFalseCount = model.TrueFalseCount,
                McqMarks = model.McqMarks,
                TrueFalseMarks = model.TrueFalseMarks,

                // Optional but recommended
                QuestionCount = (model.McqCount ?? 0) + (model.TrueFalseCount ?? 0)
            };

            _context.Exams.Add(exam);
            _context.SaveChanges();

            var instructorId = _context.Instructors
                .Select(i => i.InstructorId)
                .FirstOrDefault();

            var assign = new Assign
            {
                ExamId = exam.ExamId,
                InstructorId = instructorId,
                BranchId = model.BranchId,
                TrackId = model.TrackId
            };

            _context.Assigns.Add(assign);
            _context.SaveChanges();

            return RedirectToAction("Exams");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteExam(int examId)
        {
            var exam = await _context.Exams
                .Include(e => e.Assigns)
                .Include(e => e.Answers)
                .FirstOrDefaultAsync(e => e.ExamId == examId);

            if (exam == null)
                return NotFound();

            _context.Answers.RemoveRange(exam.Answers);
            _context.Assigns.RemoveRange(exam.Assigns);
            _context.Exams.Remove(exam);

            await _context.SaveChangesAsync();
            return RedirectToAction("Exams");
        }
    }
}
