using System;
using System.Collections.Generic;

namespace ITIExaminationSystem.Models;

public partial class Solve
{
    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public int ExamId { get; set; }

    public int? TrialNumber { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Exam Exam { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
