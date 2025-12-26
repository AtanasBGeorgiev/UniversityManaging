using System;
using System.ComponentModel.DataAnnotations;

namespace Common.Entities;

public class Course
{
    [Key]
    public int CourseID { get; set; }
    public string Title { get; set; }
    public int Credits { get; set; }
    public int FacultyID { get; set; }

    public virtual Faculty Faculty { get; set; }

    public virtual List<Professor> Professors { get; set; }
    public virtual List<Student> Students { get; set; }
}
