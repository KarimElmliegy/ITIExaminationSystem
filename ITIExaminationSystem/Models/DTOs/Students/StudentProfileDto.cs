public class StudentProfileDto
{
    public int Student_Id { get; set; }
    public int User_Id { get; set; }
    public string Student_Name { get; set; }    // ← Use this
    public string Student_Email { get; set; }   // ← Use this
    public int Track_Id { get; set; }
    public string Track_Name { get; set; }
    public int Branch_Id { get; set; }
    public string Branch_Name { get; set; }
    public int Intake_Number { get; set; }
}