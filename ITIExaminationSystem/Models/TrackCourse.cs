namespace ITIExaminationSystem.Models
{
    public class TrackCourse
    {
        public int TrackId { get; set; }
        public Track Track { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }
    }
}