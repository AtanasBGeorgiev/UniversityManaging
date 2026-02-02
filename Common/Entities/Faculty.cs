using System;
using System.ComponentModel.DataAnnotations;

namespace Common.Entities;

public class Faculty : BaseEntity
{
    [Key]
    public int FacultyID { get; set; }
    public string Name { get; set; }
    public int? DeanID { get; set; }

    public virtual List<Admin> Admins { get; set; }

    public override int[] GetIds()
    {
        return [FacultyID];
    }
}