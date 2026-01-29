namespace ITIExaminationSystem.Models.DTOs.Students
{
    public class StudentExamDto
    {
        public int ExamId { get; set; }
        public int CourseId { get; set; }

        public int FullMarks { get; set; }

        public DateOnly? Date { get; set; }
        public TimeOnly? Time { get; set; }
        public double? Duration { get; set; }

        public int QuestionCount { get; set; }

        public int StudentScore { get; set; }
        public bool IsCompleted { get; set; }
    }
}
