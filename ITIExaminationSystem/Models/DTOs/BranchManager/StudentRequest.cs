namespace ITIExaminationSystem.Models.DTOs.BranchManager
{
    public class StudentRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int BranchId { get; set; }
        public int TrackId { get; set; }
        public int IntakeNumber { get; set; }
    }
}