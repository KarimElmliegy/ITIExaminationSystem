namespace ITIExaminationSystem.Models.DTOs.BranchManager
{
    public class TrackRequest
    {
        public string TrackName { get; set; }
        public int? BranchId { get; set; }  // Add this
    }
}
