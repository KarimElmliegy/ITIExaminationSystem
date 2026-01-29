namespace ITIExaminationSystem.Models.ModelView
{
    public class StudentExamSummaryViewModel
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Date { get; set; }
        public string StartTime { get; set; }
        public DateTime? FullStartDate { get; set; } // ✅ ADD THIS for the countdown
        public bool IsExpired { get; set; }
        public double? Duration { get; set; }
        public int QuestionCount { get; set; }
        public bool Available { get; set; }
        public int? Score { get; set; }
        public int? TotalScore { get; set; }
        public bool IsCompleted { get; set; }
    }

}
