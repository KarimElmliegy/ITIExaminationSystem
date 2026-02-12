using Microsoft.AspNetCore.Mvc;
using ITIExaminationSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using ITIExaminationSystem.Models.DTOs.Admin;
using ITIExaminationSystem.Models.DTOs.AdminDashBoard;
using Microsoft.Data.SqlClient; // Required for SQL Parameters

namespace ITIExaminationSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly ExaminationSystemContext _context;

        public AdminController(ExaminationSystemContext context)
        {
            _context = context;
        }

        // GET: Admin/Dashboard - Main admin page
        // Note: We keep EF Core here for reading complex data graphs (Include) for the View.
        // Converting this GET to SPs would require changing the View logic or complex mapping.
        public IActionResult Dashboard()
        {
            ViewBag.StudentCount = _context.Students.Count();
            ViewBag.InstructorCount = _context.Instructors.Count();
            ViewBag.CourseCount = _context.Courses.Count();
            ViewBag.BranchCount = _context.Branches.Count();
            ViewBag.TrackCount = _context.Tracks.Count();

            ViewBag.Students = _context.Students
                .Include(s => s.User)
                .Include(s => s.Branch)
                .Include(s => s.Track)
                .ToList();

            ViewBag.Instructors = _context.Instructors
                .Include(i => i.User)
                .ToList();

            ViewBag.Courses = _context.Courses.ToList();
            ViewBag.Branches = _context.Branches.ToList();
            ViewBag.Tracks = _context.Tracks.ToList();

            return View();
        }

        // ================= STUDENT OPERATIONS (STORED PROCEDURES) =================

        [HttpPost]
        public IActionResult AddStudent([FromBody] StudentRequest request)
        {
            try
            {
                // Validate the request
                if (string.IsNullOrWhiteSpace(request.FullName) ||
                    string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    return Json(new { success = false, message = "Please fill all required fields" });
                }

                // Execute the stored procedure
                _context.Database.ExecuteSqlRaw(
                    "EXEC sp_Admin_AddStudent @FullName, @Email, @Password, @BranchId, @TrackId, @IntakeNumber",
                    new SqlParameter("@FullName", request.FullName),
                    new SqlParameter("@Email", request.Email),
                    new SqlParameter("@Password", request.Password),
                    new SqlParameter("@BranchId", request.BranchId),
                    new SqlParameter("@TrackId", request.TrackId),
                    new SqlParameter("@IntakeNumber", request.IntakeNumber)
                );

                return Json(new { success = true, message = "Student added successfully" });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.InnerException?.Message ?? ex.Message
                });
            }
        }
        [HttpPost]
        public IActionResult UpdateStudent([FromBody] UpdateStudentRequest request)
        {
            try
            {
                _context.Database.ExecuteSqlRaw("EXEC sp_Admin_UpdateStudent @StudentId, @FullName, @Email, @BranchId, @TrackId, @IntakeNumber",
                    new SqlParameter("@StudentId", request.StudentId),
                    new SqlParameter("@FullName", request.FullName),
                    new SqlParameter("@Email", request.Email),
                    new SqlParameter("@BranchId", request.BranchId),
                    new SqlParameter("@TrackId", request.TrackId),
                    new SqlParameter("@IntakeNumber", request.IntakeNumber)
                );

                return Json(new { success = true, message = "Student updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeleteStudent(int studentId)
        {
            try
            {
                _context.Database.ExecuteSqlRaw("EXEC sp_Admin_DeleteStudent @StudentId",
                    new SqlParameter("@StudentId", studentId));

                return Json(new { success = true, message = "Student deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.InnerException?.Message ?? ex.Message });
            }
        }

        // ================= INSTRUCTOR OPERATIONS (STORED PROCEDURES) =================

        [HttpPost]
        public IActionResult AddInstructor([FromBody] InstructorRequest request)
        {
            try
            {
                if (request.Password != request.ConfirmPassword)
                {
                    return Json(new { success = false, message = "Passwords do not match" });
                }

                _context.Database.ExecuteSqlRaw("EXEC sp_Admin_AddInstructor @FullName, @Email, @Password",
                    new SqlParameter("@FullName", request.FullName),
                    new SqlParameter("@Email", request.Email),
                    new SqlParameter("@Password", request.Password)
                );

                return Json(new { success = true, message = "Instructor created successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateInstructor([FromBody] UpdateInstructorRequest request)
        {
            try
            {
                // If password is null/empty in C#, we pass DBNull.Value to SQL
                var passwordParam = new SqlParameter("@Password", (object)request.Password ?? DBNull.Value);

                _context.Database.ExecuteSqlRaw("EXEC sp_Admin_UpdateInstructor @InstructorId, @FullName, @Email, @Password",
                    new SqlParameter("@InstructorId", request.InstructorId),
                    new SqlParameter("@FullName", request.FullName),
                    new SqlParameter("@Email", request.Email),
                    passwordParam
                );

                return Json(new { success = true, message = "Instructor updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeleteInstructor(int instructorId)
        {
            try
            {
                _context.Database.ExecuteSqlRaw("EXEC sp_Admin_DeleteInstructor @InstructorId",
                    new SqlParameter("@InstructorId", instructorId));

                return Json(new { success = true, message = "Instructor deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.InnerException?.Message ?? ex.Message });
            }
        }

        // ================= COURSE OPERATIONS (STORED PROCEDURES) =================

        [HttpPost]
        public IActionResult AddCourse([FromBody] CourseRequest request)
        {
            try
            {
                _context.Database.ExecuteSqlRaw("EXEC sp_Admin_AddCourse @CourseName, @TriesLimit",
                    new SqlParameter("@CourseName", request.CourseName),
                    new SqlParameter("@TriesLimit", request.TriesLimit ?? 0)
                );

                return Json(new { success = true, message = "Course created successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateCourse([FromBody] UpdateCourseRequest request)
        {
            try
            {
                _context.Database.ExecuteSqlRaw("EXEC sp_Admin_UpdateCourse @CourseId, @CourseName, @TriesLimit",
                    new SqlParameter("@CourseId", request.CourseId),
                    new SqlParameter("@CourseName", request.CourseName),
                    new SqlParameter("@TriesLimit", request.TriesLimit ?? 0)
                );

                return Json(new { success = true, message = "Course updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeleteCourse(int courseId)
        {
            try
            {
                _context.Database.ExecuteSqlRaw("EXEC sp_Admin_DeleteCourse @CourseId",
                    new SqlParameter("@CourseId", courseId));

                return Json(new { success = true, message = "Course deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // ================= BRANCH OPERATIONS (STORED PROCEDURES) =================

        [HttpPost]
        public IActionResult AddBranch([FromBody] BranchRequest request)
        {
            try
            {
                _context.Database.ExecuteSqlRaw("EXEC sp_Admin_AddBranch @BranchName, @BranchLocation",
                    new SqlParameter("@BranchName", request.BranchName),
                    new SqlParameter("@BranchLocation", request.BranchLocation)
                );

                return Json(new { success = true, message = "Branch created successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateBranch([FromBody] UpdateBranchRequest request)
        {
            try
            {
                _context.Database.ExecuteSqlRaw("EXEC sp_Admin_UpdateBranch @BranchId, @BranchName, @BranchLocation",
                    new SqlParameter("@BranchId", request.BranchId),
                    new SqlParameter("@BranchName", request.BranchName),
                    new SqlParameter("@BranchLocation", request.BranchLocation)
                );

                return Json(new { success = true, message = "Branch updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeleteBranch(int branchId)
        {
            try
            {
                _context.Database.ExecuteSqlRaw("EXEC sp_Admin_DeleteBranch @BranchId",
                    new SqlParameter("@BranchId", branchId));

                return Json(new { success = true, message = "Branch deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // ================= TRACK OPERATIONS (STORED PROCEDURES) =================

        [HttpPost]
        public IActionResult AddTrack([FromBody] TrackRequest request)
        {
            try
            {
                _context.Database.ExecuteSqlRaw("EXEC sp_Admin_AddTrack @TrackName",
                    new SqlParameter("@TrackName", request.TrackName)
                );

                return Json(new { success = true, message = "Track created successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateTrack([FromBody] UpdateTrackRequest request)
        {
            try
            {
                _context.Database.ExecuteSqlRaw("EXEC sp_Admin_UpdateTrack @TrackId, @TrackName",
                    new SqlParameter("@TrackId", request.TrackId),
                    new SqlParameter("@TrackName", request.TrackName)
                );

                return Json(new { success = true, message = "Track updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeleteTrack(int trackId)
        {
            try
            {
                _context.Database.ExecuteSqlRaw("EXEC sp_Admin_DeleteTrack @TrackId",
                    new SqlParameter("@TrackId", trackId));

                return Json(new { success = true, message = "Track deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}