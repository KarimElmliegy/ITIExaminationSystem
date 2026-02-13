namespace ITIExaminationSystem.Models.ModelView
{
    public class TakeExamViewModel
    {
        public int id { get; set; }
        public string Name { get; set; }
        public double Duration { get; set; }
        public int? TotalScore { get; set; }

        public int McqMarks { get; set; }
        public int TrueFalseMarks { get; set; }
        public List<ExamQuestionViewModel> QuestionList { get; set; } = new();
    }
}
