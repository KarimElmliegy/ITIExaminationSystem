namespace ITIExaminationSystem.Models.ModelView
{
    public class CourseViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Code { get; set; }
        public string Instructor { get; set; }
        public int Modules { get; set; }
        public int Exams { get; set; }
        public int Completed { get; set; }
        public string Status { get; set; }
        public List<string> Topics { get; set; }
        public string Description { get; set; }
        public List<StudentExamSummaryViewModel> ExamList { get; set; }
    }

}
