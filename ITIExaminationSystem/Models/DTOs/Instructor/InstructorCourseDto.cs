namespace ITIExaminationSystem.Models.DTOs.Instructor
{
    public class InstructorCourseDto
    {
        public int Course_Id { get; set; }
        public string Course_Name { get; set; }
        public int Duration { get; set; }

        // 🔥 Flattened instructor data
        public string InstructorName { get; set; }
        public string InstructorEmail { get; set; }
    }
}
