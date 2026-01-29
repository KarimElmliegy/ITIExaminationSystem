namespace ITIExaminationSystem.Models.DTOs.Instructor
{
    public class InstructorStudentDto
    {
        public int Student_Id { get; set; }
        public int User_Id { get; set; }
        public string User_Name { get; set; }
        public string User_Email { get; set; }
        public int Track_Id { get; set; }
        public int Branch_Id { get; set; }
        public int Intake_Number { get; set; }
    }
}
