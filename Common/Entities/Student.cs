using System;
using System.ComponentModel.DataAnnotations;

namespace Common.Entities;

public class Student : Person
{
    [Key]
    public int StudentID { get; set; }
    public int FacultyID { get; set; }
}
