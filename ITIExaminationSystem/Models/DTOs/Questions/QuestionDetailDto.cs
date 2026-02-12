namespace ITIExaminationSystem.Models.DTOs.Questions
{
    public class QuestionDetailDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }   // "MCQ" | "TF"
        public bool? CorrectTf { get; set; }        // only for TF questions
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public int InstructorId { get; set; }
        public string InstructorName { get; set; }

        // MCQ choices
        public List<ChoiceDetailDto> Choices { get; set; } = new();
    }
}
