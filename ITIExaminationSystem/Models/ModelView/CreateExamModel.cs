using System.ComponentModel.DataAnnotations;

namespace ITIExaminationSystem.Models.ModelView
{
    public class CreateExamModel
    {
        public int ExamId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public string Date { get; set; }

        [Required]
        public string Time { get; set; }

        [Required]
        public decimal Duration { get; set; }

        [Required]
        public int FullMarks { get; set; }

        public int QuestionCount { get; set; }

        // New properties for assignment
        [Required]
        public int BranchId { get; set; }

        [Required]
        public int TrackId { get; set; }

        // Optional: MCQ and True/False details
        public int? McqCount { get; set; }
        public int? McqMarks { get; set; }
        public int? TrueFalseCount { get; set; }
        public int? TrueFalseMarks { get; set; }
        public string? Instructions { get; set; }
    }
}
