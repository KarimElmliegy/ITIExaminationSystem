using System;
using System.Collections.Generic;

namespace ITIExaminationSystem.Models;

public partial class Track
{
    public int TrackId { get; set; }

    public string? TrackName { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual ICollection<Teach> Teaches { get; set; } = new List<Teach>();

    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
