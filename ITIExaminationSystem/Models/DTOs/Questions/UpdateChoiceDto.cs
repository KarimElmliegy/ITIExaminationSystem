namespace ITIExaminationSystem.Models.DTOs.Questions
{
    public class UpdateQuestionDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }
        public bool? CorrectTf { get; set; }
        public int CourseId { get; set; }

        // For MCQ: list of choices with their text and correctness flag
        public List<UpdateChoiceDto> Choices { get; set; } = new();
    }
}
