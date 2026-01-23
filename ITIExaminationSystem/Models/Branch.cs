using System;
using System.Collections.Generic;

namespace ITIExaminationSystem.Models;

public partial class Branch
{
    public int BranchId { get; set; }

    public string? BranchName { get; set; }

    public string? BranchLocation { get; set; }

    public virtual ICollection<BranchManager> BranchManagers { get; set; } = new List<BranchManager>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual ICollection<Teach> Teaches { get; set; } = new List<Teach>();

    public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();
}
