using Microsoft.AspNetCore.Mvc;
using ITIExaminationSystem.Models.ModelView;
using ITIExaminationSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ITIExaminationSystem.Controllers
{
    public class InstructorController : Controller
    {
        private readonly ILogger<InstructorController> _logger;
        private readonly ExaminationSystemContext _context;

        // ✅ CONSTRUCTOR (Dependency Injection)
        public InstructorController(
            ILogger<InstructorController> logger,
            ExaminationSystemContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

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

        public IActionResult Courses()
        {
            var ListOfCourse = _context.Courses
                .Include(c => c.Teaches)              // Load the link table
                    .ThenInclude(t => t.Instructor)   // Load the Instructor
                    .ThenInclude(i => i.User)         // Load the User (Name/Email)
                .ToList();

            ViewBag.instractors = _context.Instructors.Count();
            ViewBag.StudentCount = _context.Students.Count();
            ViewBag.CourseCount = _context.Courses.Count();

            return View("Courses", ListOfCourse);
        }
        [HttpPost]
        public IActionResult DeleteCourse(int courseId)
        {
            try
            {
                // 1. Load Course with ALL relationships
                var course = _context.Courses
                    .Include(c => c.Students)      // Student_Course
                    .Include(c => c.Tracks)        // Track_Course
                    .Include(c => c.Teaches)       // Teach
                    .Include(c => c.Topics)        // Topic
                    .Include(c => c.Solves)        // Solve
                    .Include(c => c.Exams)
                        .ThenInclude(e => e.Questions) // Exam_Questions
                    .Include(c => c.Questions)
                        .ThenInclude(q => q.Choices) // Load Choices
                    .Include(c => c.Questions)
                        .ThenInclude(q => q.ChoicesNavigation) // Question_Correct_Choices
                    .FirstOrDefault(c => c.CourseId == courseId);

                if (course == null)
                    return NotFound("Course not found");

                // Get IDs for complex queries
                var examIds = course.Exams.Select(e => e.ExamId).ToList();
                var questionIds = course.Questions.Select(q => q.QuestionId).ToList();

                // ---------------------------------------------------------
                // STEP 1: Clear Many-to-Many Relationships
                // ---------------------------------------------------------
                course.Students.Clear();  // Student_Course
                course.Tracks.Clear();    // Track_Course

                // ---------------------------------------------------------
                // STEP 2: Delete Answers (depends on Exam + Question)
                // ---------------------------------------------------------
                var relatedAnswers = _context.Answers
                    .Include(a => a.Choices) // Load Answer_Selected_Choices
                    .Where(a =>
                        (a.ExamId != null && examIds.Contains(a.ExamId.Value)) ||
                        (a.QuestionId != null && questionIds.Contains(a.QuestionId.Value))
                    ).ToList();

                // Clear many-to-many for each answer
                foreach (var answer in relatedAnswers)
                {
                    answer.Choices.Clear(); // Clears Answer_Selected_Choices
                }
                _context.Answers.RemoveRange(relatedAnswers);

                // ---------------------------------------------------------
                // STEP 3: Delete Assigns (depends on Exam)
                // ---------------------------------------------------------
                var relatedAssigns = _context.Assigns
                    .Where(a => examIds.Contains(a.ExamId));
                _context.Assigns.RemoveRange(relatedAssigns);

                // ---------------------------------------------------------
                // STEP 4: Delete Solves (depends on Course)
                // ---------------------------------------------------------
                if (course.Solves != null && course.Solves.Any())
                {
                    _context.Solves.RemoveRange(course.Solves);
                }

                // ---------------------------------------------------------
                // STEP 5: Delete Exams (after Answers/Assigns are cleared)
                // ---------------------------------------------------------
                if (course.Exams != null && course.Exams.Any())
                {
                    // Clear Exam_Questions many-to-many
                    foreach (var exam in course.Exams)
                    {
                        exam.Questions.Clear();
                    }
                    _context.Exams.RemoveRange(course.Exams);
                }

                // ---------------------------------------------------------
                // STEP 6: Delete Questions and Choices
                // ---------------------------------------------------------
                if (course.Questions != null && course.Questions.Any())
                {
                    // Clear Question_Correct_Choices many-to-many
                    foreach (var question in course.Questions)
                    {
                        question.ChoicesNavigation.Clear();
                    }

                    // Delete all Choices linked to these Questions
                    var relatedChoices = _context.Choices
                        .Where(c => c.QuestionId != null && questionIds.Contains(c.QuestionId.Value));
                    _context.Choices.RemoveRange(relatedChoices);

                    // Now delete Questions
                    _context.Questions.RemoveRange(course.Questions);
                }

                // ---------------------------------------------------------
                // STEP 7: Delete Teaches (Course-Instructor link)
                // ---------------------------------------------------------
                if (course.Teaches != null && course.Teaches.Any())
                {
                    _context.Teaches.RemoveRange(course.Teaches);
                }

                // ---------------------------------------------------------
                // STEP 8: Delete Topics
                // ---------------------------------------------------------
                if (course.Topics != null && course.Topics.Any())
                {
                    _context.Topics.RemoveRange(course.Topics);
                }

                // ---------------------------------------------------------
                // STEP 9: Finally, Delete the Course
                // ---------------------------------------------------------
                _context.Courses.Remove(course);

                // Commit all changes
                _context.SaveChanges();

                return Ok("Course deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete course failed");
                return StatusCode(500, "Database Error: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }
        [HttpPost]
        public IActionResult DeleteStudent(int userId)
        {
            try
            {
                var student = _context.Students
                    .Include(s => s.User)
                    .Include(s => s.Courses)
                    .Include(s => s.Answers)
                    .Include(s => s.Assigns)
                    .Include(s => s.Solves)
                    .FirstOrDefault(s => s.UserId == userId);

                if (student == null)
                    return NotFound("Student not found");

                // 1️⃣ Clear MANY-TO-MANY (Student_Course)
                student.Courses.Clear();

                // 2️⃣ Delete Answers (Answer_Selected_Choices handled automatically)
                _context.Answers.RemoveRange(student.Answers);

                // 3️⃣ Delete Assign
                _context.Assigns.RemoveRange(student.Assigns);

                // 4️⃣ Delete Solve
                _context.Solves.RemoveRange(student.Solves);

                // 5️⃣ Delete Student
                _context.Students.Remove(student);

                // 6️⃣ Delete User
                _context.Users.Remove(student.User);

                _context.SaveChanges();

                return Ok("Student deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete student failed");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public IActionResult AddCourse([FromBody] CourseDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid data");

            // =========================
            // EDIT EXISTING COURSE
            // =========================
            if (model.CourseId.HasValue)
            {
                var course = _context.Courses
                    .Include(c => c.Teaches)
                    .ThenInclude(t => t.Instructor)
                    .ThenInclude(i => i.User)
                    .FirstOrDefault(c => c.CourseId == model.CourseId.Value);

                if (course == null)
                    return NotFound("Course not found");

                // Update course data
                course.CourseName = model.CourseName;
                course.Duration = model.Duration;

                // Update instructor data
                var teach = course.Teaches.FirstOrDefault();
                if (teach != null)
                {
                    teach.Instructor.User.UserName = model.InstructorName;
                    teach.Instructor.User.UserEmail = model.InstructorEmail;
                }

                _context.SaveChanges();
                return Ok("Course updated successfully");
            }

            // =========================
            // ADD NEW COURSE
            // =========================
            var user = _context.Users
                .FirstOrDefault(u => u.UserEmail == model.InstructorEmail);

            Instructor instructor;

            if (user == null)
            {
                user = new User
                {
                    UserName = model.InstructorName,
                    UserEmail = model.InstructorEmail,
                    UserPassword = "123456",
                    Role = "Instructor"
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                instructor = new Instructor { UserId = user.UserId };
                _context.Instructors.Add(instructor);
                _context.SaveChanges();
            }
            else
            {
                instructor = _context.Instructors
                    .FirstOrDefault(i => i.UserId == user.UserId);

                if (instructor == null)
                    return BadRequest("User exists but is not an instructor");
            }

            var newCourse = new Course
            {
                CourseName = model.CourseName,
                Duration = model.Duration
            };

            _context.Courses.Add(newCourse);
            _context.SaveChanges();

            var teachLink = new Teach
            {
                CourseId = newCourse.CourseId,
                InstructorId = instructor.InstructorId,
                BranchId = 1,
                TrackId = 1,
                IsSupervisor = true
            };

            _context.Teaches.Add(teachLink);
            _context.SaveChanges();

            return Ok("Course added successfully");
        }

        public IActionResult AddStudent(StudentAdd s)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("DashBoard");
            }

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

            return View("insDiplayStudents", students);
        }

        [HttpPost]
        public IActionResult UpdateStudent(StudentUpdate s)
        {
            var student = _context.Students
                .Include(st => st.User)
                .FirstOrDefault(st => st.User.UserId == s.UserId);

            if (student == null)
            {
                return NotFound();
            }

            student.BranchId = s.Branch;
            student.TrackId = s.Track;
            student.IntakeNumber = s.Intake;

            student.User.UserEmail = s.Email;
            student.User.UserName = s.FullName;

            _context.SaveChanges();

            return RedirectToAction("insDiplayStudents");
        }


    }
}
