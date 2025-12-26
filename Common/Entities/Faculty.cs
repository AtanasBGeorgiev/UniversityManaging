using System;
using System.ComponentModel.DataAnnotations;

namespace Common.Entities;

public class Faculty
{
    [Key]
    public int FacultyID { get; set; }
    public string Name { get; set; }
    public int? DeanID { get; set; }
}