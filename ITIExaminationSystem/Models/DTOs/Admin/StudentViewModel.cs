namespace ITIExaminationSystem.Models.DTOs.Admin
{
    public class StudentViewModel
    {
        public int StudentId { get; set; }
        public int TrackId { get; set; }
        public int BranchId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public int IntakeNumber { get; set; }
        public string TrackName { get; set; }
        public string BranchName { get; set; }
    }
}
