namespace ITIExaminationSystem.Models.DTOs.Instructor
{
    public class InstructorExamDto
    {
        public int Exam_Id { get; set; }
        public string Course_Name { get; set; }

        public DateOnly? Date { get; set; }
        public TimeOnly? Time { get; set; }

        public double? Duration { get; set; }
        public int Full_Marks { get; set; }
        public int QuestionCount { get; set; } // ✅ ADD THIS
    }
}
