using System;
using System.Collections.Generic;

namespace ITIExaminationSystem.Models;

public partial class Teach
{
    public int CourseId { get; set; }

    public int BranchId { get; set; }

    public int InstructorId { get; set; }

    public int TrackId { get; set; }

    public bool? IsSupervisor { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual Course Course { get; set; } = null!;

    public virtual Instructor Instructor { get; set; } = null!;

    public virtual Track Track { get; set; } = null!;
}
