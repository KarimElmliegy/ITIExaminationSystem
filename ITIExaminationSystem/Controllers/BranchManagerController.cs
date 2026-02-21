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
using  ITIExaminationSystem.Models.DTOs.BranchManager;

namespace ITIExaminationSystem.Controllers
{
    public class BranchManagerController : Controller
    {
        private readonly ExaminationSystemContext _context;
        private readonly string _connectionString;

        public BranchManagerController(ExaminationSystemContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("ExaminationSystem");
        }

        // ================= DASHBOARD =================

        private async Task<List<TrackCourseDto>> GetTrackCoursesFromProcedure(int trackId)
        {
            var trackCourses = new List<TrackCourseDto>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_BranchManager_GetTrackCourses", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TrackId", trackId);  // CHANGED FROM BRANCHID TO TRACKID

                await connection.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    trackCourses.Add(new TrackCourseDto
                    {
                        TrackId = reader.GetInt32(0),
                        CourseId = reader.GetInt32(1),
                        CourseName = reader.GetString(2),
                        TrackName = reader.GetString(3)
                    });
                }

                System.Diagnostics.Debug.WriteLine($"GetTrackCoursesFromProcedure (Track {trackId}): Found {trackCourses.Count} courses");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in GetTrackCoursesFromProcedure (Track {trackId}): {ex.Message}");
            }

            return trackCourses;
        }
        public async Task<IActionResult> Dashboard(int branchId)
        {
            try
            {
                var branch = await _context.Branches.FindAsync(branchId);

                if (branch == null)
                {
                    return View("~/Views/BranchManager/Dashboard.cshtml");
                }

                ViewBag.BranchId = branchId;
                ViewBag.BranchName = branch.BranchName;

                // Get dashboard counts
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_BranchManager_GetDashboardCounts", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BranchId", branchId);

                await connection.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    ViewBag.StudentCount = reader["StudentCount"];
                    ViewBag.InstructorCount = reader["InstructorCount"];
                    ViewBag.CourseCount = reader["CourseCount"];
                    ViewBag.TrackCount = reader["TrackCount"];
                }

                // Get students
                ViewBag.Students = await GetStudentsFromProcedure(branchId);

                // Get instructors
                ViewBag.Instructors = await GetInstructorsFromProcedure();

                // Get courses
                ViewBag.Courses = await _context.Courses.ToListAsync();

                // ============ CRITICAL FIX: Get tracks for this branch ============
                var tracks = await GetTracksForBranch(branchId);
                ViewBag.Tracks = tracks;

                // ============ CRITICAL FIX: Get track-course assignments for EACH TRACK ============
                var allTrackCourses = new List<TrackCourseDto>();

                if (tracks != null && tracks.Any())
                {
                    foreach (var track in tracks)
                    {
                        // Get courses for this specific track
                        var trackCourses = await GetTrackCoursesFromProcedure(track.TrackId);
                        allTrackCourses.AddRange(trackCourses);

                        // DEBUG: Write to output window
                        System.Diagnostics.Debug.WriteLine($"Track {track.TrackId}: {track.TrackName} - {trackCourses.Count} courses");

                        foreach (var tc in trackCourses)
                        {
                            System.Diagnostics.Debug.WriteLine($"  └─ Course: {tc.CourseName} (ID: {tc.CourseId})");
                        }
                    }
                }

                // ============ STORE IN VIEWBAG ============
                ViewBag.TrackCourses = allTrackCourses;

                // ============ FINAL DEBUG ============
                System.Diagnostics.Debug.WriteLine("========== DASHBOARD FINAL ==========");
                System.Diagnostics.Debug.WriteLine($"BranchId: {branchId}");
                System.Diagnostics.Debug.WriteLine($"Tracks found: {tracks?.Count() ?? 0}");
                System.Diagnostics.Debug.WriteLine($"Total TrackCourses: {allTrackCourses.Count}");
                System.Diagnostics.Debug.WriteLine("=====================================");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in Dashboard: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ViewBag.TrackCourses = new List<TrackCourseDto>();
            }

            return View("~/Views/BranchManager/Dashboard.cshtml");
        }

        // ================= PROCEDURE DATA FETCH METHODS =================
        private async Task<List<StudentViewModel>> GetStudentsFromProcedure(int branchId)
        {
            var students = new List<StudentViewModel>();

            using var connection = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_BranchManager_GetStudents", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@BranchId", branchId);

            await connection.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                students.Add(new StudentViewModel
                {
                    StudentId = reader.GetInt32(0),
                    TrackId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                    BranchId = reader.GetInt32(2),
                    UserId = reader.GetInt32(3),
                    UserName = reader.GetString(4),
                    UserEmail = reader.GetString(5),
                    IntakeNumber = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                    TrackName = reader.IsDBNull(7) ? null : reader.GetString(7)
                });
            }

            return students;
        }

        private async Task<List<InstructorViewModel>> GetInstructorsFromProcedure()
        {
            var instructors = new List<InstructorViewModel>();

            using var connection = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_BranchManager_GetInstructors", connection);
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

        private async Task<List<TrackViewModel>> GetTracksForBranch(int branchId)
        {
            var tracks = new List<TrackViewModel>();

            using var connection = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_BranchManager_GetTracks", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@BranchId", branchId);

            await connection.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                tracks.Add(new TrackViewModel
                {
                    TrackId = reader.GetInt32(0),
                    TrackName = reader.GetString(1),
                    StudentCount = reader.IsDBNull(2) ? 0 : reader.GetInt32(2)
                });
            }

            return tracks;
        }

        // ================= STUDENT OPERATIONS =================
        [HttpPost]
        public async Task<IActionResult> AddStudent([FromBody] StudentRequest request)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_BranchManager_AddStudent", connection);
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
                using var cmd = new SqlCommand("sp_BranchManager_UpdateStudent", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@StudentId", request.StudentId);
                cmd.Parameters.AddWithValue("@FullName", request.FullName);
                cmd.Parameters.AddWithValue("@Email", request.Email);
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
                using var cmd = new SqlCommand("sp_BranchManager_DeleteStudent", connection);
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
                using var cmd = new SqlCommand("sp_BranchManager_AddInstructor", connection);
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
                using var cmd = new SqlCommand("sp_BranchManager_UpdateInstructor", connection);
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
                using var cmd = new SqlCommand("sp_BranchManager_DeleteInstructor", connection);
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
                using var cmd = new SqlCommand("sp_BranchManager_AddCourse", connection);
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
                using var cmd = new SqlCommand("sp_BranchManager_UpdateCourse", connection);
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
                using var cmd = new SqlCommand("sp_BranchManager_DeleteCourse", connection);
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

        // ================= ASSIGN COURSE TO TRACK =================
        [HttpPost]
        public async Task<IActionResult> AssignCourseToTrack(int trackId, int courseId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_BranchManager_AssignCourseToTrack", connection);
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
        // ================= TRACK OPERATIONS =================

        [HttpPost]
        public async Task<IActionResult> AddTrack([FromBody] TrackRequest request)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(request.TrackName))
                {
                    return Json(new { success = false, message = "Track name is required" });
                }

                // Get branchId from the request
                int branchId = request.BranchId ?? 0;

                if (branchId == 0)
                {
                    return Json(new { success = false, message = "Branch ID is required" });
                }

                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_BranchManager_AddTrack", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@TrackName", request.TrackName);
                cmd.Parameters.AddWithValue("@BranchId", branchId);

                var pTrackId = new SqlParameter("@TrackId", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                var pError = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, -1)
                {
                    Direction = ParameterDirection.Output
                };

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
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"SQL Error in AddTrack: {ex.Message}");
                return Json(new { success = false, message = $"Database error: {ex.Message}" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AddTrack: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTrack([FromBody] UpdateTrackRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.TrackName))
                {
                    return Json(new { success = false, message = "Track name is required" });
                }

                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_BranchManager_UpdateTrack", connection);
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
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"SQL Error in UpdateTrack: {ex.Message}");
                return Json(new { success = false, message = $"Database error: {ex.Message}" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UpdateTrack: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTrack(int trackId)
        {
            try
            {
                if (trackId <= 0)
                {
                    return Json(new { success = false, message = "Invalid track ID" });
                }

                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_BranchManager_DeleteTrack", connection);
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
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"SQL Error in DeleteTrack: {ex.Message}");
                return Json(new { success = false, message = $"Database error: {ex.Message}" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DeleteTrack: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCourseFromTrack(int trackId, int courseId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_BranchManager_RemoveCourseFromTrack", connection);
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

    // ================= VIEW MODEL CLASSES =================
    public class StudentViewModel
    {
        public int StudentId { get; set; }
        public int TrackId { get; set; }
        public int BranchId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public int IntakeNumber { get; set; }
        public string TrackName { get; set; }
        public string BranchName { get; set; }
    }

    public class InstructorViewModel
    {
        public int InstructorId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
    }

    public class TrackViewModel
    {
        public int TrackId { get; set; }
        public string TrackName { get; set; }
        public int StudentCount { get; set; }
    }

    // ================= REQUEST DTO CLASSES =================




  



    public class CourseRequest
    {
        public string CourseName { get; set; }
        public int? TriesLimit { get; set; }
        public int? Duration { get; set; }
    }

   
}