namespace ITIExaminationSystem.Models.ModelView
{
    public class StudentAdd
    {
        public string FullName { get; set; }
        public int Branch { get; set; }
        public int Track { get; set; }
        public int Intake { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
