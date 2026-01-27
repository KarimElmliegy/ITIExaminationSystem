using System;
using System.Collections.Generic;

namespace ITIExaminationSystem.Models;

public partial class Student
{
    public int StudentId { get; set; }

    public int? IntakeNumber { get; set; }

    public int? UserId { get; set; }

    public int? BranchId { get; set; }

    public int? TrackId { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();


    public virtual Branch? Branch { get; set; }

    public virtual ICollection<Solve> Solves { get; set; } = new List<Solve>();

    public virtual Track? Track { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
