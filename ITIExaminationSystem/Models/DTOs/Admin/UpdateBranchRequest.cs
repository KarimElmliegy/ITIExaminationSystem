namespace ITIExaminationSystem.Models.DTOs.Admin
{
    public class UpdateBranchRequest
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public string BranchLocation { get; set; }
    }
}
