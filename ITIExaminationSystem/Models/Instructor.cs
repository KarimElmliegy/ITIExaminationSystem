using System;
using System.Collections.Generic;

namespace ITIExaminationSystem.Models;

public partial class Instructor
{
    public int InstructorId { get; set; }

    public int? UserId { get; set; }

    public virtual ICollection<Assign> Assigns { get; set; } = new List<Assign>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<Teach> Teaches { get; set; } = new List<Teach>();

    public virtual User? User { get; set; }
}
