using System;
using System.Collections.Generic;

namespace ITIExaminationSystem.Models;

public partial class Exam
{
    public int ExamId { get; set; }

    public double? Duration { get; set; }

    public TimeOnly? Time { get; set; }

    public DateOnly? Date { get; set; }

    public int? FullMarks { get; set; }

    public int? CourseId { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual ICollection<Assign> Assigns { get; set; } = new List<Assign>();

    public virtual Course? Course { get; set; }

    public virtual ICollection<Solve> Solves { get; set; } = new List<Solve>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
