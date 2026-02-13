using System.ComponentModel.DataAnnotations.Schema;

namespace ITIExaminationSystem.Models.DTOs.Students
{
    public class CourseTopicDto
    {
        public int Topic_Id { get; set; }


        [Column("Topic_Name")]
        public string TopicName { get; set; }  // maps to Topic_Name column
    }
}
