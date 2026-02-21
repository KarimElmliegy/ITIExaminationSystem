namespace ITIExaminationSystem.Models.DTOs.Admin
{
    public class UpdateCourseRequest
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public int? TriesLimit { get; set; }
        public int? Duration { get; set; }
    }
}
