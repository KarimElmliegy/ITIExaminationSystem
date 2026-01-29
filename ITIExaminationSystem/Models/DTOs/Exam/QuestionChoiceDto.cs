namespace ITIExaminationSystem.Models.DTOs.Exam
{
    public class QuestionChoiceDto
    {
        public int Question_Id { get; set; }

        public string Question_Text { get; set; } = null!; // always exists

        public string Question_Type { get; set; } = null!; // always exists

        public int? Choice_Id { get; set; }

        public string? Choice_Text { get; set; } // ✅ MUST be nullable
    }
}
