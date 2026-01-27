namespace ITIExaminationSystem.Models.ModelView
{
    public class SubmitExamViewModel
    {
        public int ExamId { get; set; }
        public int StudentId { get; set; }

        // JSON string sent from JS
        public string AnswersJson { get; set; }
    }
}
