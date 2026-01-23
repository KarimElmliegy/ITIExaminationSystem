using System;
using System.Collections.Generic;

namespace ITIExaminationSystem.Models;

public partial class Assign
{
    public int StudentId { get; set; }

    public int ExamId { get; set; }

    public int InstructorId { get; set; }

    public virtual Exam Exam { get; set; } = null!;

    public virtual Instructor Instructor { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
