using System;
using System.Collections.Generic;

namespace ITIExaminationSystem.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? UserName { get; set; }

    public string? UserEmail { get; set; }

    public string? UserPassword { get; set; }

    public string? Role { get; set; }

    public virtual Admin? Admin { get; set; }

    public virtual ICollection<BranchManager> BranchManagers { get; set; } = new List<BranchManager>();

    public virtual Instructor? Instructor { get; set; }

    public virtual Student? Student { get; set; }
}
