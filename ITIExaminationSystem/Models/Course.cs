using System;
using System.Collections.Generic;

namespace ITIExaminationSystem.Models;

public partial class Course
{
    public int CourseId { get; set; }

    public string? CourseName { get; set; }

    public int? TriesLimit { get; set; }

    public int? Duration { get; set; }

    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<Solve> Solves { get; set; } = new List<Solve>();

    public virtual ICollection<Teach> Teaches { get; set; } = new List<Teach>();

    public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();
}
