using ITIExaminationSystem.Controllers;

namespace ITIExaminationSystem.Models.ModelView
{
    public class QuestionVM
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }

        public List<ChoiceVM> Choices { get; set; }
    }

}
