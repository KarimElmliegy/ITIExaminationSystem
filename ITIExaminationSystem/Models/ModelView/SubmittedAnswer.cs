namespace ITIExaminationSystem.Models.ModelView
{
    public class SubmittedAnswer
    {
            public int QuestionId { get; set; }
            public int? ChoiceId { get; set; }  // For MCQ
            public bool? TrueFalseAnswer { get; set; }  // For T/F
    }
}
