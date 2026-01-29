namespace ITIExaminationSystem.Models.DTOs.Admin
{
    public class UpdateStudentRequest
    {
        public int StudentId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int BranchId { get; set; }
        public int TrackId { get; set; }
        public int IntakeNumber { get; set; }
    }
}
