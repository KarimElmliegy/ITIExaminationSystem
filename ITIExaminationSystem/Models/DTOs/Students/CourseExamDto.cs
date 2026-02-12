namespace ITIExaminationSystem.Models.DTOs.Students
{
    public class CourseExamDto
    {
        public int? Exam_Id { get; set; }
        public int Course_Id { get; set; }
        public string Course_Name { get; set; }

        public DateOnly? Date { get; set; }
        public TimeOnly? Time { get; set; }
        public double? Duration { get; set; }

        public int? Full_Marks { get; set; }
        public int QuestionCount { get; set; }

        public int? StudentScore { get; set; }
        public bool IsCompleted { get; set; }
    }

}
