namespace ITIExaminationSystem.Models.ModelView
{
    public class StudentCourseDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }

        public int TopicsCount { get; set; }
        public int ExamsCount { get; set; }

        public string InstructorName { get; set; }
    }
}
