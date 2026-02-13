namespace ITIExaminationSystem.Models.ModelView
{
    public class SubmitExamViewModel
    {
        public int ExamId { get; set; }
        public int StudentId { get; set; }
        public string AnswersJson { get; set; }
        public int McqMarks { get; set; }       // ← ADD
        public int TrueFalseMarks { get; set; }  // ← ADD
    }
}

