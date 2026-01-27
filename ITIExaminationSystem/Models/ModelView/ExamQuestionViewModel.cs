namespace ITIExaminationSystem.Models.ModelView
{
    public class ExamQuestionViewModel
    {
        public int QuestionId { get; set; }
        public string Text { get; set; }
        public string Type { get; set; }
        public List<ExamChoiceViewModel> Choices { get; set; } = new();
    }
}
