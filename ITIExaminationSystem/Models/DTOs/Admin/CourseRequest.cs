namespace ITIExaminationSystem.Models.DTOs.Admin
{
    // ================= COURSE DTOs =================
    public class CourseRequest
    {
        public string CourseName { get; set; }
        public int? TriesLimit { get; set; }
    }
}
