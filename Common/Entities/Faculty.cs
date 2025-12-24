using System;
using System.ComponentModel.DataAnnotations;

namespace Common.Entities;

public class Faculty
{
    [Key]
    public int FacultyID { get; set; }
    public string Name { get; set; }
    public int DeanID { get; set; }

    public virtual Professor Dean { get; set; }
    public virtual List<Student> Students { get; set; }
    public virtual List<Professor> Professors { get; set; }
    public virtual List<Course> Courses { get; set; }
}
