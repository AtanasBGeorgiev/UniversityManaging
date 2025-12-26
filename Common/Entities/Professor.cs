using System;
using System.ComponentModel.DataAnnotations;

namespace Common.Entities;

public class Professor : Person
{
    [Key]
    public int ProfessorID { get; set; }
    public int FacultyID { get; set; }

    public virtual Faculty Faculty { get; set; }
    public virtual Faculty DeanOfFaculty { get; set; }
}