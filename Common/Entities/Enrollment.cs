using System;

namespace Common.Entities;

public class Enrollment
{
    public int StudentID { get; set; }
    public int CourseID { get; set; }
    public string Year { get; set; }
    public int Semester { get; set; }
    public int? Grade { get; set; }

    public virtual Student Student { get; set; }
    public virtual Course Course { get; set; }
}
