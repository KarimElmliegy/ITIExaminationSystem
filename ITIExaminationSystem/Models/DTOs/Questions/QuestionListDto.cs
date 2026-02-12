namespace ITIExaminationSystem.Models.DTOs.Questions
{
    public class ChoiceDetailDto
    {
        public int ChoiceId { get; set; }
        public string ChoiceText { get; set; }
        public bool IsCorrect { get; set; }   // true if this choice is in Question_Correct_Choices
    }
}
