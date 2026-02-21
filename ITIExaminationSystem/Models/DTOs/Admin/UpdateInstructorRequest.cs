namespace ITIExaminationSystem.Models.DTOs.Admin
{
    public class UpdateInstructorRequest
    {
        public int InstructorId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

}
