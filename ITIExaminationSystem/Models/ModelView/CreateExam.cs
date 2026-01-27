namespace ITIExaminationSystem.ViewModels;

public class CreateExamViewModel
{
    public int ExamId { get; set; }
    public int? CourseId { get; set; }
    public string Date { get; set; }           // String to receive from form
    public string Time { get; set; }           // String to receive from form
    public double? Duration { get; set; }
    public int? FullMarks { get; set; }
    public int? McqCount { get; set; }
    public int? McqMarks { get; set; }

    public int QuestionCount { get; set; }
    public int? TrueFalseCount { get; set; }
    public int? TrueFalseMarks { get; set; }
    public string Instructions { get; set; }
}