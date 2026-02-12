namespace ITIExaminationSystem.Models.DTOs.Questions
{
    public class QuestionListDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }   // "MCQ" | "TF"
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public int InstructorId { get; set; }
        public string InstructorName { get; set; }
        public int ChoiceCount { get; set; }
    }
}
