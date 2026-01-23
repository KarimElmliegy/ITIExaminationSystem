using System;
using System.Collections.Generic;

namespace ITIExaminationSystem.Models;

public partial class Question
{
    public int QuestionId { get; set; }

    public string? QuestionText { get; set; }

    public string? QuestionType { get; set; }

    public bool? CorrectTf { get; set; }

    public int? InstructorId { get; set; }

    public int? CourseId { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual ICollection<Choice> Choices { get; set; } = new List<Choice>();

    public virtual Course? Course { get; set; }

    public virtual Instructor? Instructor { get; set; }

    public virtual ICollection<Choice> ChoicesNavigation { get; set; } = new List<Choice>();

    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();
}
