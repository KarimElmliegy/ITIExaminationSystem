namespace ITIExaminationSystem.Models.DTOs.Questions
{
    public class UpdateChoiceDto
    {
        public int? ChoiceId { get; set; }   // null = new choice
        public string ChoiceText { get; set; }
        public bool IsCorrect { get; set; }
        public bool IsDeleted { get; set; }  // true = remove this choice
    }
}
