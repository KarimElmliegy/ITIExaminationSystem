namespace ITIExaminationSystem.Models.ModelView
{
    public class CourseDto
    {
        public int? CourseId { get; set; }   // 👈 REQUIRED

        public string CourseName { get; set; }
        public string InstructorName { get; set; }

        public string InstructorEmail { get; set; }

        public int Duration { get; set; }
    }
}