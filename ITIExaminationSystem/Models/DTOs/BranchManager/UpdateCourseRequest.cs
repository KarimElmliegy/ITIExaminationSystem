// Models/DTOs/BranchManager/UpdateCourseRequest.cs
namespace ITIExaminationSystem.Models.DTOs.BranchManager
{
    public class UpdateCourseRequest
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public int? TriesLimit { get; set; }
        public int? Duration { get; set; }
    }
}

