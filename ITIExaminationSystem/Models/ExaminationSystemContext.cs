using System;
using System.Collections.Generic;
using ITIExaminationSystem.Models.DTOs.AdminDashBoard;
using ITIExaminationSystem.Models.DTOs.Exam;
using ITIExaminationSystem.Models.DTOs.Instructor;
using ITIExaminationSystem.Models.DTOs.Students;
using ITIExaminationSystem.Models.Login;
using Microsoft.EntityFrameworkCore;

namespace ITIExaminationSystem.Models;


public partial class ExaminationSystemContext : DbContext
{
    public ExaminationSystemContext()
    {
    }


    /// <summary>
    /// 
    /// public DbSet<StudentDashboardDto> StudentDashboardDtos { get; set; }
    ///public DbSet<InstructorDashboardDto> InstructorDashboardDtos { get; set; }
    ///public DbSet<DashboardCountDto> DashboardCountDtos { get; set; }
    /// </summary>
    /// 
    public DbSet<StudentDashboardDto> StudentDashboardDtos { get; set; }
    public DbSet<InstructorDashboardDto> InstructorDashboardDtos { get; set; }
    public DbSet<DashboardCountDto> DashboardCountDtos { get; set; }

    public DbSet<StudentProfileDto> StudentProfileDtos { get; set; }
    public DbSet<CourseInstructorDto> CourseInstructorDtos { get; set; }
    public DbSet<CourseExamDto> CourseExamDtos { get; set; }

    public DbSet<InstructorStudentDto> InstructorStudentDtos { get; set; }
    public DbSet<InstructorExamDto> InstructorExamDtos { get; set; }
    public DbSet<LoginUserDto> LoginUserDtos { get; set; }

    public DbSet<ExamAccessDto> ExamAccessDtos { get; set; }
    public DbSet<QuestionChoiceDto> QuestionChoiceDtos { get; set; }

    public DbSet<InstructorCourseDto> InstructorCourseDtos { get; set; }

    public DbSet<TrackCourseCountDto> TrackCourseCountDtos { get; set; }

    public DbSet<UpdateExamDto> UpdateExamDtos { get; set; }


    public DbSet<CourseTopicDto> CourseTopicDtos { get; set; }































    public ExaminationSystemContext(DbContextOptions<ExaminationSystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Answer> Answers { get; set; }

    public virtual DbSet<Assign> Assigns { get; set; }

    public virtual DbSet<Branch> Branches { get; set; }

    public virtual DbSet<BranchManager> BranchManagers { get; set; }

    public virtual DbSet<Choice> Choices { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Exam> Exams { get; set; }

    public virtual DbSet<Instructor> Instructors { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Solve> Solves { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Teach> Teaches { get; set; }

    public virtual DbSet<Topic> Topics { get; set; }

    public virtual DbSet<Track> Tracks { get; set; }
   // public DbSet<TrackCourse> TrackCourses { get; set; }
    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=ExaminationSystem2003;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {


        modelBuilder.Entity<StudentDashboardDto>().HasNoKey();
        modelBuilder.Entity<InstructorDashboardDto>().HasNoKey();
        modelBuilder.Entity<DashboardCountDto>().HasNoKey();

        modelBuilder.Entity<CourseExamDto>().HasNoKey();
        modelBuilder.Entity<CourseInstructorDto>().HasNoKey();
        modelBuilder.Entity<StudentProfileDto>().HasNoKey();

        modelBuilder.Entity<InstructorStudentDto>().HasNoKey();
        modelBuilder.Entity<InstructorCourseDto>().HasNoKey();
        modelBuilder.Entity<InstructorExamDto>().HasNoKey();
        modelBuilder.Entity<LoginUserDto>().HasNoKey();
        modelBuilder.Entity<ExamAccessDto>().HasNoKey();
        modelBuilder.Entity<QuestionChoiceDto>().HasNoKey();
        modelBuilder.Entity<InstructorCourseDto>().HasNoKey();

        modelBuilder.Entity<TrackCourseCountDto>().HasNoKey();
        modelBuilder.Entity<UpdateExamDto>().HasNoKey();

        modelBuilder.Entity<CourseTopicDto>().HasNoKey().ToView(null);


        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.AdminId).HasName("PK__Admin__4A3006F760C3DE7A");

            entity.ToTable("Admin");

            entity.HasIndex(e => e.UserId, "UQ__Admin__206D9171021E5F58").IsUnique();

            entity.Property(e => e.AdminId).HasColumnName("Admin_Id");
            entity.Property(e => e.UserId).HasColumnName("User_Id");

            entity.HasOne(d => d.User).WithOne(p => p.Admin)
                .HasForeignKey<Admin>(d => d.UserId)
                .HasConstraintName("FK__Admin__User_Id__3B75D760");
        });


        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.AnswerId).HasName("PK__Answer__36918F389C12DAC8");

            entity.ToTable("Answer");

            entity.Property(e => e.AnswerId).HasColumnName("Answer_Id");
            entity.Property(e => e.ExamId).HasColumnName("Exam_Id");
            entity.Property(e => e.QuestionId).HasColumnName("Question_Id");
            entity.Property(e => e.ScoredMarks).HasColumnName("Scored_Marks");
            entity.Property(e => e.StudentId).HasColumnName("Student_Id");
            entity.Property(e => e.TfSelected).HasColumnName("TF_Selected");

            entity.HasOne(d => d.Exam).WithMany(p => p.Answers)
                .HasForeignKey(d => d.ExamId)
                .HasConstraintName("FK__Answer__Exam_Id__76969D2E");

            entity.HasOne(d => d.Question).WithMany(p => p.Answers)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK__Answer__Question__778AC167");

            entity.HasOne(d => d.Student).WithMany(p => p.Answers)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Answer__Student___75A278F5");

            entity.HasMany(d => d.Choices).WithMany(p => p.Answers)
                .UsingEntity<Dictionary<string, object>>(
                    "AnswerSelectedChoice",
                    r => r.HasOne<Choice>().WithMany()
                        .HasForeignKey("ChoiceId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Answer_Se__Choic__7B5B524B"),
                    l => l.HasOne<Answer>().WithMany()
                        .HasForeignKey("AnswerId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Answer_Se__Answe__7A672E12"),
                    j =>
                    {
                        j.HasKey("AnswerId", "ChoiceId").HasName("PK__Answer_S__8BFD02979955B5C0");
                        j.ToTable("Answer_Selected_Choices");
                        j.IndexerProperty<int>("AnswerId").HasColumnName("Answer_Id");
                        j.IndexerProperty<int>("ChoiceId").HasColumnName("Choice_Id");
                    });
        });

        modelBuilder.Entity<Assign>(entity =>
        {
            // 1. Define the new Composite Primary Key
            entity.HasKey(e => new { e.ExamId, e.InstructorId, e.BranchId, e.TrackId })
                  .HasName("PK_Assign_New");

            entity.ToTable("Assign");

            entity.Property(e => e.ExamId).HasColumnName("Exam_Id");
            entity.Property(e => e.InstructorId).HasColumnName("Instructor_Id");
            entity.Property(e => e.BranchId).HasColumnName("Branch_Id");
            entity.Property(e => e.TrackId).HasColumnName("Track_Id");

            // 2. Setup Relationships
            entity.HasOne(d => d.Exam)
                .WithMany(p => p.Assigns)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Assign_Exam");

            entity.HasOne(d => d.Instructor)
                .WithMany(p => p.Assigns)
                .HasForeignKey(d => d.InstructorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Assign_Instructor");

            entity.HasOne(d => d.Branch)
                .WithMany(p => p.Assigns) // Ensure Branch class has public virtual ICollection<Assign> Assigns { get; set; }
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Assign_Branch");

            entity.HasOne(d => d.Track)
                .WithMany(p => p.Assigns) // Ensure Track class has public virtual ICollection<Assign> Assigns { get; set; }
                .HasForeignKey(d => d.TrackId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Assign_Track");
        });
        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.BranchId).HasName("PK__Branch__12CEB061AD589742");

            entity.ToTable("Branch");

            entity.Property(e => e.BranchId).HasColumnName("Branch_Id");
            entity.Property(e => e.BranchLocation)
                .HasMaxLength(100)
                .HasColumnName("Branch_Location");
            entity.Property(e => e.BranchName)
                .HasMaxLength(100)
                .HasColumnName("Branch_Name");

            entity.HasMany(d => d.Tracks).WithMany(p => p.Branches)
                .UsingEntity<Dictionary<string, object>>(
                    "BranchTrack",
                    r => r.HasOne<Track>().WithMany()
                        .HasForeignKey("TrackId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Branch_Tr__Track__4E88ABD4"),
                    l => l.HasOne<Branch>().WithMany()
                        .HasForeignKey("BranchId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Branch_Tr__Branc__4D94879B"),
                    j =>
                    {
                        j.HasKey("BranchId", "TrackId").HasName("PK__Branch_T__E714720B152DF829");
                        j.ToTable("Branch_Track");
                        j.IndexerProperty<int>("BranchId").HasColumnName("Branch_Id");
                        j.IndexerProperty<int>("TrackId").HasColumnName("Track_Id");
                    });
        });

        modelBuilder.Entity<BranchManager>(entity =>
        {
            entity.HasKey(e => e.ManagerId).HasName("PK__Branch_M__AE5FEFAD89C7F12F");

            entity.ToTable("Branch_Manager");

            entity.Property(e => e.ManagerId).HasColumnName("Manager_Id");
            entity.Property(e => e.BranchId).HasColumnName("Branch_Id");
            entity.Property(e => e.UserId).HasColumnName("User_Id");

            entity.HasOne(d => d.Branch).WithMany(p => p.BranchManagers)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK__Branch_Ma__Branc__4AB81AF0");

            entity.HasOne(d => d.User).WithMany(p => p.BranchManagers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Branch_Ma__User___49C3F6B7");
        });

        modelBuilder.Entity<Choice>(entity =>
        {
            entity.HasKey(e => e.ChoiceId).HasName("PK__Choice__D6C8DAFAFF01FA83");

            entity.ToTable("Choice");

            entity.Property(e => e.ChoiceId).HasColumnName("Choice_Id");
            entity.Property(e => e.ChoiceText)
                .HasMaxLength(255)
                .HasColumnName("Choice_Text");
            entity.Property(e => e.QuestionId).HasColumnName("Question_Id");

            entity.HasOne(d => d.Question).WithMany(p => p.Choices)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK__Choice__Question__6B24EA82");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Course__37E005DB603B40BB");

            entity.ToTable("Course");

            entity.Property(e => e.CourseId).HasColumnName("Course_Id");
            entity.Property(e => e.CourseName)
                .HasMaxLength(100)
                .HasColumnName("Course_Name");
            entity.Property(e => e.TriesLimit).HasColumnName("Tries_Limit");
        });

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasKey(e => e.ExamId).HasName("PK__Exam__C782CA59A3A9FB14");

            entity.ToTable("Exam");

            entity.Property(e => e.QuestionCount)
                 .HasDefaultValue(0);

            entity.Property(e => e.ExamId).HasColumnName("Exam_Id");
            entity.Property(e => e.CourseId).HasColumnName("Course_Id");
            entity.Property(e => e.FullMarks).HasColumnName("Full_Marks");

            entity.HasOne(d => d.Course).WithMany(p => p.Exams)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__Exam__Course_Id__5FB337D6");

            entity.HasMany(d => d.Questions).WithMany(p => p.Exams)
                .UsingEntity<Dictionary<string, object>>(
                    "ExamQuestion",
                    r => r.HasOne<Question>().WithMany()
                        .HasForeignKey("QuestionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Exam_Ques__Quest__72C60C4A"),
                    l => l.HasOne<Exam>().WithMany()
                        .HasForeignKey("ExamId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Exam_Ques__Exam___71D1E811"),
                    j =>
                    {
                        j.HasKey("ExamId", "QuestionId").HasName("PK__Exam_Que__BC89E417FCCD2AF7");
                        j.ToTable("Exam_Questions");
                        j.IndexerProperty<int>("ExamId").HasColumnName("Exam_Id");
                        j.IndexerProperty<int>("QuestionId").HasColumnName("Question_Id");
                    });
        });

        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.HasKey(e => e.InstructorId).HasName("PK__Instruct__DD4B9AEAE6A8A24C");

            entity.ToTable("Instructor");

            entity.HasIndex(e => e.UserId, "UQ__Instruct__206D91715F36F848").IsUnique();

            entity.Property(e => e.InstructorId).HasColumnName("Instructor_Id");
            entity.Property(e => e.UserId).HasColumnName("User_Id");

            entity.HasOne(d => d.User).WithOne(p => p.Instructor)
                .HasForeignKey<Instructor>(d => d.UserId)
                .HasConstraintName("FK__Instructo__User___3F466844");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Question__B0B2E4E64CCE7EB8");

            entity.ToTable("Question");

            entity.Property(e => e.QuestionId).HasColumnName("Question_Id");
            entity.Property(e => e.CorrectTf).HasColumnName("Correct_TF");
            entity.Property(e => e.CourseId).HasColumnName("Course_Id");
            entity.Property(e => e.InstructorId).HasColumnName("Instructor_Id");
            entity.Property(e => e.QuestionText).HasColumnName("Question_Text");
            entity.Property(e => e.QuestionType)
                .HasMaxLength(50)
                .HasColumnName("Question_Type");

            entity.HasOne(d => d.Course).WithMany(p => p.Questions)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__Question__Course__68487DD7");

            entity.HasOne(d => d.Instructor).WithMany(p => p.Questions)
                .HasForeignKey(d => d.InstructorId)
                .HasConstraintName("FK__Question__Instru__6754599E");

            entity.HasMany(d => d.ChoicesNavigation).WithMany(p => p.Questions)
                .UsingEntity<Dictionary<string, object>>(
                    "QuestionCorrectChoice",
                    r => r.HasOne<Choice>().WithMany()
                        .HasForeignKey("ChoiceId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Question___Choic__6EF57B66"),
                    l => l.HasOne<Question>().WithMany()
                        .HasForeignKey("QuestionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Question___Quest__6E01572D"),
                    j =>
                    {
                        j.HasKey("QuestionId", "ChoiceId").HasName("PK__Question__0DDE69496828E75B");
                        j.ToTable("Question_Correct_Choices");
                        j.IndexerProperty<int>("QuestionId").HasColumnName("Question_Id");
                        j.IndexerProperty<int>("ChoiceId").HasColumnName("Choice_Id");
                    });
        });

        modelBuilder.Entity<Solve>(entity =>
        {
            entity.HasKey(e => new { e.StudentId, e.CourseId, e.ExamId }).HasName("PK__Solve__2E8CC52939432065");

            entity.ToTable("Solve");

            entity.Property(e => e.StudentId).HasColumnName("Student_Id");
            entity.Property(e => e.CourseId).HasColumnName("Course_Id");
            entity.Property(e => e.ExamId).HasColumnName("Exam_Id");
            entity.Property(e => e.TrialNumber).HasColumnName("Trial_Number");

            entity.HasOne(d => d.Course).WithMany(p => p.Solves)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Solve__Course_Id__7F2BE32F");

            entity.HasOne(d => d.Exam).WithMany(p => p.Solves)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Solve__Exam_Id__00200768");

            entity.HasOne(d => d.Student).WithMany(p => p.Solves)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Solve__Student_I__7E37BEF6");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Student__A2F4E98CC78B11FB");

            entity.ToTable("Student");

            entity.HasIndex(e => e.UserId, "UQ__Student__206D91713C485A67").IsUnique();

            entity.Property(e => e.StudentId).HasColumnName("Student_Id");
            entity.Property(e => e.BranchId).HasColumnName("Branch_Id");
            entity.Property(e => e.IntakeNumber).HasColumnName("Intake_Number");
            entity.Property(e => e.TrackId).HasColumnName("Track_Id");
            entity.Property(e => e.UserId).HasColumnName("User_Id");

            entity.HasOne(d => d.Branch).WithMany(p => p.Students)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK_Student_Branch");

            entity.HasOne(d => d.Track).WithMany(p => p.Students)
                .HasForeignKey(d => d.TrackId)
                .HasConstraintName("FK_Student_Track");

            entity.HasOne(d => d.User).WithOne(p => p.Student)
                .HasForeignKey<Student>(d => d.UserId)
                .HasConstraintName("FK__Student__User_Id__4316F928");

            entity.HasMany(d => d.Courses).WithMany(p => p.Students)
                .UsingEntity<Dictionary<string, object>>(
                    "StudentCourse",
                    r => r.HasOne<Course>().WithMany()
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Student_C__Cours__5CD6CB2B"),
                    l => l.HasOne<Student>().WithMany()
                        .HasForeignKey("StudentId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Student_C__Stude__5BE2A6F2"),
                    j =>
                    {
                        j.HasKey("StudentId", "CourseId").HasName("PK__Student___018AE9D1D0EA719D");
                        j.ToTable("Student_Course");
                        j.IndexerProperty<int>("StudentId").HasColumnName("Student_Id");
                        j.IndexerProperty<int>("CourseId").HasColumnName("Course_Id");
                    });
        });

        modelBuilder.Entity<Teach>(entity =>
        {
            entity.HasKey(e => new { e.CourseId, e.BranchId, e.InstructorId, e.TrackId }).HasName("PK__Teach__DD11A547E6AC0E77");

            entity.ToTable("Teach");

            entity.Property(e => e.CourseId).HasColumnName("Course_Id");
            entity.Property(e => e.BranchId).HasColumnName("Branch_Id");
            entity.Property(e => e.InstructorId).HasColumnName("Instructor_Id");
            entity.Property(e => e.TrackId).HasColumnName("Track_Id");
            entity.Property(e => e.IsSupervisor).HasColumnName("Is_Supervisor");

            entity.HasOne(d => d.Branch).WithMany(p => p.Teaches)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Teach__Branch_Id__571DF1D5");

            entity.HasOne(d => d.Course).WithMany(p => p.Teaches)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Teach__Course_Id__5629CD9C");

            entity.HasOne(d => d.Instructor).WithMany(p => p.Teaches)
                .HasForeignKey(d => d.InstructorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Teach__Instructo__5812160E");

            entity.HasOne(d => d.Track).WithMany(p => p.Teaches)
                .HasForeignKey(d => d.TrackId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Teach__Track_Id__59063A47");
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(e => e.TopicId).HasName("PK__Topic__8DEAA405428BF83B");

            entity.ToTable("Topic");

            entity.Property(e => e.TopicId).HasColumnName("Topic_Id");
            entity.Property(e => e.CourseId).HasColumnName("Course_Id");
            entity.Property(e => e.TopicName)
                .HasMaxLength(100)
                .HasColumnName("Topic_Name");

            entity.HasOne(d => d.Course).WithMany(p => p.Topics)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__Topic__Course_Id__534D60F1");
        });

        modelBuilder.Entity<Track>(entity =>
        {
            entity.HasKey(e => e.TrackId).HasName("PK__Track__5DAC26AE6C203B0A");

            entity.ToTable("Track");

            entity.Property(e => e.TrackId).HasColumnName("Track_Id");
            entity.Property(e => e.TrackName)
                .HasMaxLength(100)
                .HasColumnName("Track_Name");

            entity.HasMany(d => d.Courses).WithMany(p => p.Tracks)
                .UsingEntity<Dictionary<string, object>>(
                    "TrackCourse",
                    r => r.HasOne<Course>().WithMany()
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Track_Cou__Cours__25518C17"),
                    l => l.HasOne<Track>().WithMany()
                        .HasForeignKey("TrackId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Track_Cou__Track__245D67DE"),
                    j =>
                    {
                        j.HasKey("TrackId", "CourseId").HasName("PK__Track_Co__FED226F3995B4287");
                        j.ToTable("Track_Course");
                        j.IndexerProperty<int>("TrackId").HasColumnName("Track_Id");
                        j.IndexerProperty<int>("CourseId").HasColumnName("Course_Id");
                    });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__206D9170ED1EC7C3");

            entity.ToTable("User");

            entity.HasIndex(e => e.UserEmail, "UQ__User__4C70A05C7FFFAF06").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("User_Id");
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.UserEmail)
                .HasMaxLength(100)
                .HasColumnName("User_Email");
            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .HasColumnName("User_Name");
            entity.Property(e => e.UserPassword)
                .HasMaxLength(255)
                .HasColumnName("User_Password");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
