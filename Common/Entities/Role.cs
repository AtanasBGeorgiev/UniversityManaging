using System;
using System.ComponentModel.DataAnnotations;

namespace Common.Entities;

public class Roel
{
    [Key]
    public string Role { get; set; }

    public virtual List<Admin> Admins { get; set; }
    public virtual List<Student> Students { get; set; }
    public virtual List<Professor> Professors { get; set; }
}
