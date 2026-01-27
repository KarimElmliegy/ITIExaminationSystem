using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITIExaminationSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAssignToBranchTrack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Branch",
                columns: table => new
                {
                    Branch_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Branch_Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Branch_Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Branch__12CEB061AD589742", x => x.Branch_Id);
                });

            migrationBuilder.CreateTable(
                name: "Course",
                columns: table => new
                {
                    Course_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Course_Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Tries_Limit = table.Column<int>(type: "int", nullable: true),
                    Duration = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Course__37E005DB603B40BB", x => x.Course_Id);
                });

            migrationBuilder.CreateTable(
                name: "Track",
                columns: table => new
                {
                    Track_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Track_Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Track__5DAC26AE6C203B0A", x => x.Track_Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    User_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User_Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    User_Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    User_Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__User__206D9170ED1EC7C3", x => x.User_Id);
                });

            migrationBuilder.CreateTable(
                name: "Exam",
                columns: table => new
                {
                    Exam_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Duration = table.Column<double>(type: "float", nullable: true),
                    Time = table.Column<TimeOnly>(type: "time", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: true),
                    Full_Marks = table.Column<int>(type: "int", nullable: true),
                    Course_Id = table.Column<int>(type: "int", nullable: true),
                    QuestionCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Exam__C782CA59A3A9FB14", x => x.Exam_Id);
                    table.ForeignKey(
                        name: "FK__Exam__Course_Id__5FB337D6",
                        column: x => x.Course_Id,
                        principalTable: "Course",
                        principalColumn: "Course_Id");
                });

            migrationBuilder.CreateTable(
                name: "Topic",
                columns: table => new
                {
                    Topic_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Topic_Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Course_Id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Topic__8DEAA405428BF83B", x => x.Topic_Id);
                    table.ForeignKey(
                        name: "FK__Topic__Course_Id__534D60F1",
                        column: x => x.Course_Id,
                        principalTable: "Course",
                        principalColumn: "Course_Id");
                });

            migrationBuilder.CreateTable(
                name: "Branch_Track",
                columns: table => new
                {
                    Branch_Id = table.Column<int>(type: "int", nullable: false),
                    Track_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Branch_T__E714720B152DF829", x => new { x.Branch_Id, x.Track_Id });
                    table.ForeignKey(
                        name: "FK__Branch_Tr__Branc__4D94879B",
                        column: x => x.Branch_Id,
                        principalTable: "Branch",
                        principalColumn: "Branch_Id");
                    table.ForeignKey(
                        name: "FK__Branch_Tr__Track__4E88ABD4",
                        column: x => x.Track_Id,
                        principalTable: "Track",
                        principalColumn: "Track_Id");
                });

            migrationBuilder.CreateTable(
                name: "Track_Course",
                columns: table => new
                {
                    Track_Id = table.Column<int>(type: "int", nullable: false),
                    Course_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Track_Co__FED226F3995B4287", x => new { x.Track_Id, x.Course_Id });
                    table.ForeignKey(
                        name: "FK__Track_Cou__Cours__25518C17",
                        column: x => x.Course_Id,
                        principalTable: "Course",
                        principalColumn: "Course_Id");
                    table.ForeignKey(
                        name: "FK__Track_Cou__Track__245D67DE",
                        column: x => x.Track_Id,
                        principalTable: "Track",
                        principalColumn: "Track_Id");
                });

            migrationBuilder.CreateTable(
                name: "Admin",
                columns: table => new
                {
                    Admin_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User_Id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Admin__4A3006F760C3DE7A", x => x.Admin_Id);
                    table.ForeignKey(
                        name: "FK__Admin__User_Id__3B75D760",
                        column: x => x.User_Id,
                        principalTable: "User",
                        principalColumn: "User_Id");
                });

            migrationBuilder.CreateTable(
                name: "Branch_Manager",
                columns: table => new
                {
                    Manager_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User_Id = table.Column<int>(type: "int", nullable: true),
                    Branch_Id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Branch_M__AE5FEFAD89C7F12F", x => x.Manager_Id);
                    table.ForeignKey(
                        name: "FK__Branch_Ma__Branc__4AB81AF0",
                        column: x => x.Branch_Id,
                        principalTable: "Branch",
                        principalColumn: "Branch_Id");
                    table.ForeignKey(
                        name: "FK__Branch_Ma__User___49C3F6B7",
                        column: x => x.User_Id,
                        principalTable: "User",
                        principalColumn: "User_Id");
                });

            migrationBuilder.CreateTable(
                name: "Instructor",
                columns: table => new
                {
                    Instructor_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User_Id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Instruct__DD4B9AEAE6A8A24C", x => x.Instructor_Id);
                    table.ForeignKey(
                        name: "FK__Instructo__User___3F466844",
                        column: x => x.User_Id,
                        principalTable: "User",
                        principalColumn: "User_Id");
                });

            migrationBuilder.CreateTable(
                name: "Student",
                columns: table => new
                {
                    Student_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Intake_Number = table.Column<int>(type: "int", nullable: true),
                    User_Id = table.Column<int>(type: "int", nullable: true),
                    Branch_Id = table.Column<int>(type: "int", nullable: true),
                    Track_Id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Student__A2F4E98CC78B11FB", x => x.Student_Id);
                    table.ForeignKey(
                        name: "FK_Student_Branch",
                        column: x => x.Branch_Id,
                        principalTable: "Branch",
                        principalColumn: "Branch_Id");
                    table.ForeignKey(
                        name: "FK_Student_Track",
                        column: x => x.Track_Id,
                        principalTable: "Track",
                        principalColumn: "Track_Id");
                    table.ForeignKey(
                        name: "FK__Student__User_Id__4316F928",
                        column: x => x.User_Id,
                        principalTable: "User",
                        principalColumn: "User_Id");
                });

            migrationBuilder.CreateTable(
                name: "Assign",
                columns: table => new
                {
                    Exam_Id = table.Column<int>(type: "int", nullable: false),
                    Instructor_Id = table.Column<int>(type: "int", nullable: false),
                    Branch_Id = table.Column<int>(type: "int", nullable: false),
                    Track_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assign_New", x => new { x.Exam_Id, x.Instructor_Id, x.Branch_Id, x.Track_Id });
                    table.ForeignKey(
                        name: "FK_Assign_Branch",
                        column: x => x.Branch_Id,
                        principalTable: "Branch",
                        principalColumn: "Branch_Id");
                    table.ForeignKey(
                        name: "FK_Assign_Exam",
                        column: x => x.Exam_Id,
                        principalTable: "Exam",
                        principalColumn: "Exam_Id");
                    table.ForeignKey(
                        name: "FK_Assign_Instructor",
                        column: x => x.Instructor_Id,
                        principalTable: "Instructor",
                        principalColumn: "Instructor_Id");
                    table.ForeignKey(
                        name: "FK_Assign_Track",
                        column: x => x.Track_Id,
                        principalTable: "Track",
                        principalColumn: "Track_Id");
                });

            migrationBuilder.CreateTable(
                name: "Question",
                columns: table => new
                {
                    Question_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Question_Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Question_Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Correct_TF = table.Column<bool>(type: "bit", nullable: true),
                    Instructor_Id = table.Column<int>(type: "int", nullable: true),
                    Course_Id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Question__B0B2E4E64CCE7EB8", x => x.Question_Id);
                    table.ForeignKey(
                        name: "FK__Question__Course__68487DD7",
                        column: x => x.Course_Id,
                        principalTable: "Course",
                        principalColumn: "Course_Id");
                    table.ForeignKey(
                        name: "FK__Question__Instru__6754599E",
                        column: x => x.Instructor_Id,
                        principalTable: "Instructor",
                        principalColumn: "Instructor_Id");
                });

            migrationBuilder.CreateTable(
                name: "Teach",
                columns: table => new
                {
                    Course_Id = table.Column<int>(type: "int", nullable: false),
                    Branch_Id = table.Column<int>(type: "int", nullable: false),
                    Instructor_Id = table.Column<int>(type: "int", nullable: false),
                    Track_Id = table.Column<int>(type: "int", nullable: false),
                    Is_Supervisor = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Teach__DD11A547E6AC0E77", x => new { x.Course_Id, x.Branch_Id, x.Instructor_Id, x.Track_Id });
                    table.ForeignKey(
                        name: "FK__Teach__Branch_Id__571DF1D5",
                        column: x => x.Branch_Id,
                        principalTable: "Branch",
                        principalColumn: "Branch_Id");
                    table.ForeignKey(
                        name: "FK__Teach__Course_Id__5629CD9C",
                        column: x => x.Course_Id,
                        principalTable: "Course",
                        principalColumn: "Course_Id");
                    table.ForeignKey(
                        name: "FK__Teach__Instructo__5812160E",
                        column: x => x.Instructor_Id,
                        principalTable: "Instructor",
                        principalColumn: "Instructor_Id");
                    table.ForeignKey(
                        name: "FK__Teach__Track_Id__59063A47",
                        column: x => x.Track_Id,
                        principalTable: "Track",
                        principalColumn: "Track_Id");
                });

            migrationBuilder.CreateTable(
                name: "Solve",
                columns: table => new
                {
                    Student_Id = table.Column<int>(type: "int", nullable: false),
                    Course_Id = table.Column<int>(type: "int", nullable: false),
                    Exam_Id = table.Column<int>(type: "int", nullable: false),
                    Trial_Number = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Solve__2E8CC52939432065", x => new { x.Student_Id, x.Course_Id, x.Exam_Id });
                    table.ForeignKey(
                        name: "FK__Solve__Course_Id__7F2BE32F",
                        column: x => x.Course_Id,
                        principalTable: "Course",
                        principalColumn: "Course_Id");
                    table.ForeignKey(
                        name: "FK__Solve__Exam_Id__00200768",
                        column: x => x.Exam_Id,
                        principalTable: "Exam",
                        principalColumn: "Exam_Id");
                    table.ForeignKey(
                        name: "FK__Solve__Student_I__7E37BEF6",
                        column: x => x.Student_Id,
                        principalTable: "Student",
                        principalColumn: "Student_Id");
                });

            migrationBuilder.CreateTable(
                name: "Student_Course",
                columns: table => new
                {
                    Student_Id = table.Column<int>(type: "int", nullable: false),
                    Course_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Student___018AE9D1D0EA719D", x => new { x.Student_Id, x.Course_Id });
                    table.ForeignKey(
                        name: "FK__Student_C__Cours__5CD6CB2B",
                        column: x => x.Course_Id,
                        principalTable: "Course",
                        principalColumn: "Course_Id");
                    table.ForeignKey(
                        name: "FK__Student_C__Stude__5BE2A6F2",
                        column: x => x.Student_Id,
                        principalTable: "Student",
                        principalColumn: "Student_Id");
                });

            migrationBuilder.CreateTable(
                name: "Answer",
                columns: table => new
                {
                    Answer_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TF_Selected = table.Column<bool>(type: "bit", nullable: true),
                    Scored_Marks = table.Column<int>(type: "int", nullable: true),
                    Student_Id = table.Column<int>(type: "int", nullable: true),
                    Exam_Id = table.Column<int>(type: "int", nullable: true),
                    Question_Id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Answer__36918F389C12DAC8", x => x.Answer_Id);
                    table.ForeignKey(
                        name: "FK__Answer__Exam_Id__76969D2E",
                        column: x => x.Exam_Id,
                        principalTable: "Exam",
                        principalColumn: "Exam_Id");
                    table.ForeignKey(
                        name: "FK__Answer__Question__778AC167",
                        column: x => x.Question_Id,
                        principalTable: "Question",
                        principalColumn: "Question_Id");
                    table.ForeignKey(
                        name: "FK__Answer__Student___75A278F5",
                        column: x => x.Student_Id,
                        principalTable: "Student",
                        principalColumn: "Student_Id");
                });

            migrationBuilder.CreateTable(
                name: "Choice",
                columns: table => new
                {
                    Choice_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Choice_Text = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Question_Id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Choice__D6C8DAFAFF01FA83", x => x.Choice_Id);
                    table.ForeignKey(
                        name: "FK__Choice__Question__6B24EA82",
                        column: x => x.Question_Id,
                        principalTable: "Question",
                        principalColumn: "Question_Id");
                });

            migrationBuilder.CreateTable(
                name: "Exam_Questions",
                columns: table => new
                {
                    Exam_Id = table.Column<int>(type: "int", nullable: false),
                    Question_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Exam_Que__BC89E417FCCD2AF7", x => new { x.Exam_Id, x.Question_Id });
                    table.ForeignKey(
                        name: "FK__Exam_Ques__Exam___71D1E811",
                        column: x => x.Exam_Id,
                        principalTable: "Exam",
                        principalColumn: "Exam_Id");
                    table.ForeignKey(
                        name: "FK__Exam_Ques__Quest__72C60C4A",
                        column: x => x.Question_Id,
                        principalTable: "Question",
                        principalColumn: "Question_Id");
                });

            migrationBuilder.CreateTable(
                name: "Answer_Selected_Choices",
                columns: table => new
                {
                    Answer_Id = table.Column<int>(type: "int", nullable: false),
                    Choice_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Answer_S__8BFD02979955B5C0", x => new { x.Answer_Id, x.Choice_Id });
                    table.ForeignKey(
                        name: "FK__Answer_Se__Answe__7A672E12",
                        column: x => x.Answer_Id,
                        principalTable: "Answer",
                        principalColumn: "Answer_Id");
                    table.ForeignKey(
                        name: "FK__Answer_Se__Choic__7B5B524B",
                        column: x => x.Choice_Id,
                        principalTable: "Choice",
                        principalColumn: "Choice_Id");
                });

            migrationBuilder.CreateTable(
                name: "Question_Correct_Choices",
                columns: table => new
                {
                    Question_Id = table.Column<int>(type: "int", nullable: false),
                    Choice_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Question__0DDE69496828E75B", x => new { x.Question_Id, x.Choice_Id });
                    table.ForeignKey(
                        name: "FK__Question___Choic__6EF57B66",
                        column: x => x.Choice_Id,
                        principalTable: "Choice",
                        principalColumn: "Choice_Id");
                    table.ForeignKey(
                        name: "FK__Question___Quest__6E01572D",
                        column: x => x.Question_Id,
                        principalTable: "Question",
                        principalColumn: "Question_Id");
                });

            migrationBuilder.CreateIndex(
                name: "UQ__Admin__206D9171021E5F58",
                table: "Admin",
                column: "User_Id",
                unique: true,
                filter: "[User_Id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Answer_Exam_Id",
                table: "Answer",
                column: "Exam_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Answer_Question_Id",
                table: "Answer",
                column: "Question_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Answer_Student_Id",
                table: "Answer",
                column: "Student_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Answer_Selected_Choices_Choice_Id",
                table: "Answer_Selected_Choices",
                column: "Choice_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Assign_Branch_Id",
                table: "Assign",
                column: "Branch_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Assign_Instructor_Id",
                table: "Assign",
                column: "Instructor_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Assign_Track_Id",
                table: "Assign",
                column: "Track_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Branch_Manager_Branch_Id",
                table: "Branch_Manager",
                column: "Branch_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Branch_Manager_User_Id",
                table: "Branch_Manager",
                column: "User_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Branch_Track_Track_Id",
                table: "Branch_Track",
                column: "Track_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Choice_Question_Id",
                table: "Choice",
                column: "Question_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_Course_Id",
                table: "Exam",
                column: "Course_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_Questions_Question_Id",
                table: "Exam_Questions",
                column: "Question_Id");

            migrationBuilder.CreateIndex(
                name: "UQ__Instruct__206D91715F36F848",
                table: "Instructor",
                column: "User_Id",
                unique: true,
                filter: "[User_Id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Question_Course_Id",
                table: "Question",
                column: "Course_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Question_Instructor_Id",
                table: "Question",
                column: "Instructor_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Question_Correct_Choices_Choice_Id",
                table: "Question_Correct_Choices",
                column: "Choice_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Solve_Course_Id",
                table: "Solve",
                column: "Course_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Solve_Exam_Id",
                table: "Solve",
                column: "Exam_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Student_Branch_Id",
                table: "Student",
                column: "Branch_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Student_Track_Id",
                table: "Student",
                column: "Track_Id");

            migrationBuilder.CreateIndex(
                name: "UQ__Student__206D91713C485A67",
                table: "Student",
                column: "User_Id",
                unique: true,
                filter: "[User_Id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Student_Course_Course_Id",
                table: "Student_Course",
                column: "Course_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Teach_Branch_Id",
                table: "Teach",
                column: "Branch_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Teach_Instructor_Id",
                table: "Teach",
                column: "Instructor_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Teach_Track_Id",
                table: "Teach",
                column: "Track_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Topic_Course_Id",
                table: "Topic",
                column: "Course_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Track_Course_Course_Id",
                table: "Track_Course",
                column: "Course_Id");

            migrationBuilder.CreateIndex(
                name: "UQ__User__4C70A05C7FFFAF06",
                table: "User",
                column: "User_Email",
                unique: true,
                filter: "[User_Email] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admin");

            migrationBuilder.DropTable(
                name: "Answer_Selected_Choices");

            migrationBuilder.DropTable(
                name: "Assign");

            migrationBuilder.DropTable(
                name: "Branch_Manager");

            migrationBuilder.DropTable(
                name: "Branch_Track");

            migrationBuilder.DropTable(
                name: "Exam_Questions");

            migrationBuilder.DropTable(
                name: "Question_Correct_Choices");

            migrationBuilder.DropTable(
                name: "Solve");

            migrationBuilder.DropTable(
                name: "Student_Course");

            migrationBuilder.DropTable(
                name: "Teach");

            migrationBuilder.DropTable(
                name: "Topic");

            migrationBuilder.DropTable(
                name: "Track_Course");

            migrationBuilder.DropTable(
                name: "Answer");

            migrationBuilder.DropTable(
                name: "Choice");

            migrationBuilder.DropTable(
                name: "Exam");

            migrationBuilder.DropTable(
                name: "Student");

            migrationBuilder.DropTable(
                name: "Question");

            migrationBuilder.DropTable(
                name: "Branch");

            migrationBuilder.DropTable(
                name: "Track");

            migrationBuilder.DropTable(
                name: "Course");

            migrationBuilder.DropTable(
                name: "Instructor");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
