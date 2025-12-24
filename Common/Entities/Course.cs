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
}
