using ITIExaminationSystem.Controllers;
using ITIExaminationSystem.Models;
using ITIExaminationSystem.Models.ModelView;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITIExaminationSystem.Controllers

{
    public class AdminController : Controller
    {
        // 1. Initialize the context MANUALLY here
        private readonly ILogger<HomeController> _logger;
        private readonly ExaminationSystemContext context;
        // 2. REMOVED the Constructor that was causing the "Unable to resolve service" error.
        // public AdminController(ApplicationDbContext context) { ... }  <-- DELETED

        [HttpGet]
        public IActionResult Dashboard()
        {
            // Now 'context' works because we created it with 'new' above
            var students = context.Students
                .Include(s => s.User)
                .ToList();

            var instructors = context.Instructors
                .Include(i => i.User)
                .ToList();

            ViewBag.Students = students;
            ViewBag.Instructors = instructors;
            ViewBag.StudentCount = students.Count;
            ViewBag.InstructorCount = instructors.Count;

            return View();
        }

        [HttpPost]
        public IActionResult AddInstructor(instructorAdd s)
        {
            if (ModelState.IsValid)
            {
                User user = new User
                {
                    UserEmail = s.Email,
                    UserName = s.FullName,
                    UserPassword = s.Password,
                    Role = "instrcutor"
                };

                Instructor ins = new Instructor
                {
                    User = user
                };
                context.Instructors.Add(ins);
                context.SaveChanges();

                return RedirectToAction("Dashboard");
            }

            return RedirectToAction("Dashboard");
        }
        public IActionResult AddStudent(StudentAdd s)
        {
            if (ModelState.IsValid)
            {
                User user = new User
                {
                    UserEmail = s.Email,
                    UserName = s.FullName,
                    UserPassword = s.Password,
                    Role = "Student"
                };

                Student student = new Student
                {
                    TrackId = s.Track,
                    BranchId = s.Branch,
                };

                context.Students.Add(student);
                context.SaveChanges();

                return RedirectToAction("Dashboard");
            }

            return RedirectToAction("Dashboard");
        }

        public IActionResult Logout()
        {
            // HttpContext.Session.Clear(); // Uncomment if using session
            return RedirectToAction("Login", "Account");
        }
    }
}