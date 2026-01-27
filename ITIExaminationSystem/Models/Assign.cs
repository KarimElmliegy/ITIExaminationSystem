using System;
using System.Collections.Generic;

namespace ITIExaminationSystem.Models;

public partial class Assign
{
    // New Composite Key Properties
    public int ExamId { get; set; }
    public int InstructorId { get; set; }
    public int BranchId { get; set; }
    public int TrackId { get; set; }

    // Navigation Properties
    public virtual Exam Exam { get; set; } = null!;
    public virtual Instructor Instructor { get; set; } = null!;
    public virtual Branch Branch { get; set; } = null!;
    public virtual Track Track { get; set; } = null!;
}