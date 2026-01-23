using System;
using System.Collections.Generic;

namespace ITIExaminationSystem.Models;

public partial class BranchManager
{
    public int ManagerId { get; set; }

    public int? UserId { get; set; }

    public int? BranchId { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual User? User { get; set; }
}
