namespace ITIExaminationSystem.Models.DTOs.Admin
{
    public class InstructorRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

}
