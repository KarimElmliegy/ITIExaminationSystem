using System;
using System.Collections.Generic;

namespace ITIExaminationSystem.Models;

public partial class Choice
{
    public int ChoiceId { get; set; }

    public string? ChoiceText { get; set; }

    public int? QuestionId { get; set; }

    public virtual Question? Question { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
