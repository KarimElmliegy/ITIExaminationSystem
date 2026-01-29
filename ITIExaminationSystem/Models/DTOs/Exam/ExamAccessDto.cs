namespace ITIExaminationSystem.Models.DTOs.Exam
{
    public class ExamAccessDto
    {
        public int Exam_Id { get; set; }
        public int Course_Id { get; set; }
        public int? McqCount { get; set; }
        public int? TrueFalseCount { get; set; }
        public double? Duration { get; set; }
        public int Full_Marks { get; set; }
    }
}
