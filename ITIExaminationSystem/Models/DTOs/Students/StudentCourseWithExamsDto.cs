namespace ITIExaminationSystem.Models.DTOs.Students
{
    public class StudentCourseWithExamsDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }

        public int TopicsCount { get; set; }
        public string InstructorName { get; set; }

        public List<StudentExamDto> Exams { get; set; } = new();
    }
}
