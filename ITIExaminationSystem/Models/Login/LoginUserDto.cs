namespace ITIExaminationSystem.Models.Login
{
    public class LoginUserDto
    {
        public int User_Id { get; set; }
        public string User_Email { get; set; }
        public string Role { get; set; }

        public int? Branch_Id { get; set; }   // only for Branch Manager
    }
}
