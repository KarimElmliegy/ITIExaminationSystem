using System;
using System.Collections.Generic;

namespace ITIExaminationSystem.Models;

public partial class Answer
{
    public int AnswerId { get; set; }

    public bool? TfSelected { get; set; }

    public int? ScoredMarks { get; set; }

    public int? StudentId { get; set; }

    public int? ExamId { get; set; }

    public int? QuestionId { get; set; }

    public virtual Exam? Exam { get; set; }

    public virtual Question? Question { get; set; }

    public virtual Student? Student { get; set; }

    public virtual ICollection<Choice> Choices { get; set; } = new List<Choice>();
}
