using Microsoft.AspNetCore.Mvc;
using ITIExaminationSystem.Models;
using ITIExaminationSystem.Controllers; 
using ITIExaminationSystem.Models.ModelView;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using ITIExaminationSystem.Models.DTOs.Instructor;

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
            var students = _context.InstructorStudentDtos
                .FromSqlRaw("EXEC sp_Instructor_GetAllStudents")
                .ToList();

            ViewBag.Tracks = _context.Tracks.ToList(); // simple EF ok
            ViewBag.instractors = _context.Instructors.Count();
            ViewBag.StudentCount = students.Count;
            ViewBag.CourseCount = _context.Courses.Count();

            return View("insDiplayStudents", students);
        }

        public IActionResult FilterByIntake(int? intake)
        {
            int selectedIntake = intake ?? 46;
            ViewBag.SelectedIntake = selectedIntake;

            var students = _context.InstructorStudentDtos
                .FromSqlRaw(
                    "EXEC sp_Instructor_GetStudentsByIntake @Intake",
                    new SqlParameter("@Intake", selectedIntake)
                )
                .ToList();

            ViewBag.Tracks = _context.Tracks.ToList();
            return View("insDiplayStudents", students);
        }

        [HttpPost]
        public IActionResult AddStudent(StudentAdd s)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_Instructor_AddStudent @Name,@Email,@Password,@Track,@Branch,@Intake",
                new SqlParameter("@Name", s.FullName),
                new SqlParameter("@Email", s.Email),
                new SqlParameter("@Password", s.Password),
                new SqlParameter("@Track", s.Track),
                new SqlParameter("@Branch", s.Branch),
                new SqlParameter("@Intake", s.Intake)
            );

            return RedirectToAction("insDiplayStudents");
        }

        [HttpPost]
        public IActionResult UpdateStudent(StudentUpdate s)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_Instructor_UpdateStudent @UserId,@Name,@Email,@Track,@Branch,@Intake",
                new SqlParameter("@UserId", s.UserId),
                new SqlParameter("@Name", s.FullName),
                new SqlParameter("@Email", s.Email),
                new SqlParameter("@Track", s.Track),
                new SqlParameter("@Branch", s.Branch),
                new SqlParameter("@Intake", s.Intake)
            );

            return RedirectToAction("insDiplayStudents");
        }

        [HttpPost]
        public IActionResult DeleteStudent(int userId)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_Instructor_DeleteStudent @UserId",
                new SqlParameter("@UserId", userId)
            );

            return Ok();
        }

        // =========================
        // COURSES
        // =========================

        public IActionResult Courses()
        {
            var courses = _context.Set<InstructorCourseDto>()
                .FromSqlRaw("EXEC sp_Instructor_GetCourses")
                .ToList();

            return View("Courses", courses);
        }

        [HttpPost]
        public IActionResult AddCourse([FromBody] CourseDto model)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_Instructor_SaveCourse @Id,@Name,@Duration",
                new SqlParameter("@Id", model.CourseId ?? (object)DBNull.Value),
                new SqlParameter("@Name", model.CourseName),
                new SqlParameter("@Duration", model.Duration)
            );

            return Ok();
        }


        [HttpPost]
        public IActionResult EditCourse([FromBody] EditCourseDto model)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_Instructor_SaveCourse @CourseId,@Name,@Duration",
                new SqlParameter("@CourseId", model.CourseId),
                new SqlParameter("@Name", model.CourseName),
                new SqlParameter("@Duration", model.Duration)
            );

            return Ok();
        }

        [HttpPost]
        public IActionResult DeleteCourse(int courseId)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_Instructor_DeleteCourse @CourseId",
                new SqlParameter("@CourseId", courseId)
            );

            return Ok();
        }


        // =========================
        // EXAMS
        // =========================

        public IActionResult Exams()
        {
            var exams = _context.InstructorExamDtos
                .FromSqlRaw("EXEC sp_Instructor_GetExams")
                .ToList();

            ViewBag.Courses = _context.Courses.ToList();   // ok to stay EF
            ViewBag.Branches = _context.Branches.ToList();
            ViewBag.Tracks = _context.Tracks.ToList();

            return View("CreateExam", exams);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateExam(CreateExamModel model)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_Instructor_CreateExam @Course,@Date,@Time,@Duration,@Full,@McqC,@TfC,@McqM,@TfM,@Branch,@Track",
                new SqlParameter("@Course", model.CourseId),
                new SqlParameter("@Date", DateOnly.Parse(model.Date)),
                new SqlParameter("@Time", TimeOnly.Parse(model.Time)),
                new SqlParameter("@Duration", model.Duration),
                new SqlParameter("@Full", model.FullMarks),
                new SqlParameter("@McqC", model.McqCount),
                new SqlParameter("@TfC", model.TrueFalseCount),
                new SqlParameter("@McqM", model.McqMarks),
                new SqlParameter("@TfM", model.TrueFalseMarks),
                new SqlParameter("@Branch", model.BranchId),
                new SqlParameter("@Track", model.TrackId)
            );

            return RedirectToAction("Exams");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteExam(int examId)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_Instructor_DeleteExam @ExamId",
                new SqlParameter("@ExamId", examId)
            );

            return RedirectToAction("Exams");
        }

    }
}
