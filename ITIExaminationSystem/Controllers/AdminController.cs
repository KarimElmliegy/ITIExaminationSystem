using Microsoft.AspNetCore.Mvc;
using ITIExaminationSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using ITIExaminationSystem.Models.DTOs.Admin;

namespace ITIExaminationSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly ExaminationSystemContext _context;
        private readonly string _connectionString;

        public AdminController(ExaminationSystemContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("ExaminationSystem");
        }

        // ================= DASHBOARD =================

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                // Get dashboard counts using stored procedure
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_Admin_GetDashboardCounts", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                await connection.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    ViewBag.StudentCount = reader["StudentCount"];
                    ViewBag.InstructorCount = reader["InstructorCount"];
                    ViewBag.CourseCount = reader["CourseCount"];
                    ViewBag.BranchCount = reader["BranchCount"];
                    ViewBag.TrackCount = reader["TrackCount"];
                }

                // Get students using stored procedure
                ViewBag.Students = await GetStudentsFromProcedure();

                // Get instructors using stored procedure
                ViewBag.Instructors = await GetInstructorsFromProcedure();

                // Get courses, branches, tracks
                ViewBag.Courses = await _context.Courses.ToListAsync();
                ViewBag.Branches = await _context.Branches.ToListAsync();
                ViewBag.Tracks = await _context.Tracks.ToListAsync();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
            }

            return View();
        }

        // ================= PROCEDURE DATA FETCH METHODS =================

        private async Task<List<StudentViewModel>> GetStudentsFromProcedure()
        {
            var students = new List<StudentViewModel>();

            using var connection = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_Admin_GetStudents", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            await connection.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                students.Add(new StudentViewModel
                {
                    StudentId = reader.GetInt32(0),
                    TrackId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                    BranchId = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                    UserId = reader.GetInt32(3),
                    UserName = reader.GetString(4),
                    UserEmail = reader.GetString(5),
                    IntakeNumber = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                    TrackName = reader.IsDBNull(7) ? null : reader.GetString(7),
                    BranchName = reader.IsDBNull(8) ? null : reader.GetString(8)
                });
            }

            return students;
        }

        private async Task<List<InstructorViewModel>> GetInstructorsFromProcedure()
        {
            var instructors = new List<InstructorViewModel>();

            using var connection = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_Admin_GetInstructors", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            await connection.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                instructors.Add(new InstructorViewModel
                {
                    InstructorId = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    UserName = reader.GetString(2),
                    UserEmail = reader.GetString(3)
                });
            }

            return instructors;
        }

        // ================= STUDENT OPERATIONS =================

        [HttpPost]
        public async Task<IActionResult> AddStudent([FromBody] StudentRequest request)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_Admin_AddStudent", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UserName", request.FullName);
                cmd.Parameters.AddWithValue("@UserEmail", request.Email);
                cmd.Parameters.AddWithValue("@UserPassword", request.Password);
                cmd.Parameters.AddWithValue("@TrackId", request.TrackId);
                cmd.Parameters.AddWithValue("@BranchId", request.BranchId);
                cmd.Parameters.AddWithValue("@IntakeNumber", request.IntakeNumber);

                var pUserId = new SqlParameter("@UserId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var pStudentId = new SqlParameter("@StudentId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var pError = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pUserId);
                cmd.Parameters.Add(pStudentId);
                cmd.Parameters.Add(pError);

                await connection.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                string errorMessage = pError.Value?.ToString();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    return Json(new { success = false, message = errorMessage });
                }

                return Json(new
                {
                    success = true,
                    message = "Student created successfully",
                    studentId = pStudentId.Value,
                    userId = pUserId.Value
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStudent([FromBody] UpdateStudentRequest request)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_Admin_UpdateStudent", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@StudentId", request.StudentId);
                cmd.Parameters.AddWithValue("@FullName", request.FullName);
                cmd.Parameters.AddWithValue("@Email", request.Email);
                cmd.Parameters.AddWithValue("@BranchId", request.BranchId);
                cmd.Parameters.AddWithValue("@TrackId", request.TrackId);
                cmd.Parameters.AddWithValue("@IntakeNumber", request.IntakeNumber);

                var pSuccess = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                var pError = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pSuccess);
                cmd.Parameters.Add(pError);

                await connection.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                bool success = pSuccess.Value != DBNull.Value && (bool)pSuccess.Value;
                string errorMessage = pError.Value?.ToString();

                if (!success || !string.IsNullOrEmpty(errorMessage))
                {
                    return Json(new { success = false, message = errorMessage ?? "Update failed" });
                }

                return Json(new { success = true, message = "Student updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStudent(int studentId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_Admin_DeleteStudent", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@StudentId", studentId);

                var pSuccess = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                var pError = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pSuccess);
                cmd.Parameters.Add(pError);

                await connection.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                bool success = pSuccess.Value != DBNull.Value && (bool)pSuccess.Value;
                string errorMessage = pError.Value?.ToString();

                if (!success || !string.IsNullOrEmpty(errorMessage))
                {
                    return Json(new { success = false, message = errorMessage ?? "Delete failed" });
                }

                return Json(new { success = true, message = "Student deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // ================= INSTRUCTOR OPERATIONS =================

        [HttpPost]
        public async Task<IActionResult> AddInstructor([FromBody] InstructorRequest request)
        {
            try
            {
                if (request.Password != request.ConfirmPassword)
                {
                    return Json(new { success = false, message = "Passwords do not match" });
                }

                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_Admin_AddInstructor", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UserName", request.FullName);
                cmd.Parameters.AddWithValue("@UserEmail", request.Email);
                cmd.Parameters.AddWithValue("@UserPassword", request.Password);

                var pUserId = new SqlParameter("@UserId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var pInstructorId = new SqlParameter("@InstructorId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var pError = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pUserId);
                cmd.Parameters.Add(pInstructorId);
                cmd.Parameters.Add(pError);

                await connection.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                string errorMessage = pError.Value?.ToString();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    return Json(new { success = false, message = errorMessage });
                }

                return Json(new
                {
                    success = true,
                    message = "Instructor created successfully",
                    instructorId = pInstructorId.Value,
                    userId = pUserId.Value
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateInstructor([FromBody] UpdateInstructorRequest request)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_Admin_UpdateInstructor", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@InstructorId", request.InstructorId);
                cmd.Parameters.AddWithValue("@FullName", request.FullName);
                cmd.Parameters.AddWithValue("@Email", request.Email);
                cmd.Parameters.AddWithValue("@Password", (object)request.Password ?? DBNull.Value);

                var pSuccess = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                var pError = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pSuccess);
                cmd.Parameters.Add(pError);

                await connection.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                bool success = pSuccess.Value != DBNull.Value && (bool)pSuccess.Value;
                string errorMessage = pError.Value?.ToString();

                if (!success || !string.IsNullOrEmpty(errorMessage))
                {
                    return Json(new { success = false, message = errorMessage ?? "Update failed" });
                }

                return Json(new { success = true, message = "Instructor updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteInstructor(int instructorId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_Admin_DeleteInstructor", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@InstructorId", instructorId);

                var pSuccess = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                var pError = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pSuccess);
                cmd.Parameters.Add(pError);

                await connection.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                bool success = pSuccess.Value != DBNull.Value && (bool)pSuccess.Value;
                string errorMessage = pError.Value?.ToString();

                if (!success || !string.IsNullOrEmpty(errorMessage))
                {
                    return Json(new { success = false, message = errorMessage ?? "Delete failed" });
                }

                return Json(new { success = true, message = "Instructor deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // ================= COURSE OPERATIONS =================

        [HttpPost]
        public async Task<IActionResult> AddCourse([FromBody] CourseRequest request)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_Admin_AddCourse", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@CourseName", request.CourseName);
                cmd.Parameters.AddWithValue("@TriesLimit", request.TriesLimit ?? 3);
                cmd.Parameters.AddWithValue("@Duration", request.Duration ?? 40);

                var pCourseId = new SqlParameter("@CourseId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var pError = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pCourseId);
                cmd.Parameters.Add(pError);

                await connection.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                string errorMessage = pError.Value?.ToString();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    return Json(new { success = false, message = errorMessage });
                }

                return Json(new
                {
                    success = true,
                    message = "Course created successfully",
                    courseId = pCourseId.Value
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCourse([FromBody] UpdateCourseRequest request)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_Admin_UpdateCourse", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@CourseId", request.CourseId);
                cmd.Parameters.AddWithValue("@CourseName", request.CourseName);
                cmd.Parameters.AddWithValue("@TriesLimit", request.TriesLimit ?? 3);
                cmd.Parameters.AddWithValue("@Duration", request.Duration ?? 40);

                var pSuccess = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                var pError = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pSuccess);
                cmd.Parameters.Add(pError);

                await connection.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                bool success = pSuccess.Value != DBNull.Value && (bool)pSuccess.Value;
                string errorMessage = pError.Value?.ToString();

                if (!success || !string.IsNullOrEmpty(errorMessage))
                {
                    return Json(new { success = false, message = errorMessage ?? "Update failed" });
                }

                return Json(new { success = true, message = "Course updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCourse(int courseId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_Admin_DeleteCourse", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@CourseId", courseId);

                var pSuccess = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                var pError = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pSuccess);
                cmd.Parameters.Add(pError);

                await connection.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                bool success = pSuccess.Value != DBNull.Value && (bool)pSuccess.Value;
                string errorMessage = pError.Value?.ToString();

                if (!success || !string.IsNullOrEmpty(errorMessage))
                {
                    return Json(new { success = false, message = errorMessage ?? "Delete failed" });
                }

                return Json(new { success = true, message = "Course deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // ================= BRANCH OPERATIONS =================

        [HttpPost]
        public async Task<IActionResult> AddBranch([FromBody] BranchRequest request)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_Admin_AddBranch", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@BranchName", request.BranchName);
                cmd.Parameters.AddWithValue("@BranchLocation", request.BranchLocation ?? "");

                var pBranchId = new SqlParameter("@BranchId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var pError = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pBranchId);
                cmd.Parameters.Add(pError);

                await connection.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                string errorMessage = pError.Value?.ToString();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    return Json(new { success = false, message = errorMessage });
                }

                return Json(new
                {
                    success = true,
                    message = "Branch created successfully",
                    branchId = pBranchId.Value
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBranch([FromBody] UpdateBranchRequest request)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_Admin_UpdateBranch", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@BranchId", request.BranchId);
                cmd.Parameters.AddWithValue("@BranchName", request.BranchName);
                cmd.Parameters.AddWithValue("@BranchLocation", request.BranchLocation ?? "");

                var pSuccess = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                var pError = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pSuccess);
                cmd.Parameters.Add(pError);

                await connection.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                bool success = pSuccess.Value != DBNull.Value && (bool)pSuccess.Value;
                string errorMessage = pError.Value?.ToString();

                if (!success || !string.IsNullOrEmpty(errorMessage))
                {
                    return Json(new { success = false, message = errorMessage ?? "Update failed" });
                }

                return Json(new { success = true, message = "Branch updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBranch(int branchId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_Admin_DeleteBranch", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@BranchId", branchId);

                var pSuccess = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                var pError = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pSuccess);
                cmd.Parameters.Add(pError);

                await connection.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                bool success = pSuccess.Value != DBNull.Value && (bool)pSuccess.Value;
                string errorMessage = pError.Value?.ToString();

                if (!success || !string.IsNullOrEmpty(errorMessage))
                {
                    return Json(new { success = false, message = errorMessage ?? "Delete failed" });
                }

                return Json(new { success = true, message = "Branch deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // ================= TRACK OPERATIONS =================

        [HttpPost]
        public async Task<IActionResult> AddTrack([FromBody] TrackRequest request)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_Admin_AddTrack", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@TrackName", request.TrackName);

                var pTrackId = new SqlParameter("@TrackId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var pError = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pTrackId);
                cmd.Parameters.Add(pError);

                await connection.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                string errorMessage = pError.Value?.ToString();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    return Json(new { success = false, message = errorMessage });
                }

                return Json(new
                {
                    success = true,
                    message = "Track created successfully",
                    trackId = pTrackId.Value
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTrack([FromBody] UpdateTrackRequest request)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_Admin_UpdateTrack", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@TrackId", request.TrackId);
                cmd.Parameters.AddWithValue("@TrackName", request.TrackName);

                var pSuccess = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                var pError = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pSuccess);
                cmd.Parameters.Add(pError);

                await connection.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                bool success = pSuccess.Value != DBNull.Value && (bool)pSuccess.Value;
                string errorMessage = pError.Value?.ToString();

                if (!success || !string.IsNullOrEmpty(errorMessage))
                {
                    return Json(new { success = false, message = errorMessage ?? "Update failed" });
                }

                return Json(new { success = true, message = "Track updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTrack(int trackId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_Admin_DeleteTrack", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@TrackId", trackId);

                var pSuccess = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                var pError = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pSuccess);
                cmd.Parameters.Add(pError);

                await connection.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                bool success = pSuccess.Value != DBNull.Value && (bool)pSuccess.Value;
                string errorMessage = pError.Value?.ToString();

                if (!success || !string.IsNullOrEmpty(errorMessage))
                {
                    return Json(new { success = false, message = errorMessage ?? "Delete failed" });
                }

                return Json(new { success = true, message = "Track deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // ================= ASSIGN COURSE TO TRACK =================

        [HttpPost]
        public async Task<IActionResult> AssignCourseToTrack(int trackId, int courseId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_Admin_AssignCourseToTrack", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@TrackId", trackId);
                cmd.Parameters.AddWithValue("@CourseId", courseId);

                var pSuccess = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                var pError = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pSuccess);
                cmd.Parameters.Add(pError);

                await connection.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                bool success = pSuccess.Value != DBNull.Value && (bool)pSuccess.Value;
                string errorMessage = pError.Value?.ToString();

                if (!success || !string.IsNullOrEmpty(errorMessage))
                {
                    return Json(new { success = false, message = errorMessage ?? "Assignment failed" });
                }

                return Json(new { success = true, message = "Course assigned to track successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCourseFromTrack(int trackId, int courseId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_Admin_RemoveCourseFromTrack", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@TrackId", trackId);
                cmd.Parameters.AddWithValue("@CourseId", courseId);

                var pSuccess = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                var pError = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pSuccess);
                cmd.Parameters.Add(pError);

                await connection.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                bool success = pSuccess.Value != DBNull.Value && (bool)pSuccess.Value;
                string errorMessage = pError.Value?.ToString();

                if (!success || !string.IsNullOrEmpty(errorMessage))
                {
                    return Json(new { success = false, message = errorMessage ?? "Remove failed" });
                }

                return Json(new { success = true, message = "Course removed from track successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }

}