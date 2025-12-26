using System;
using System.ComponentModel.DataAnnotations;

namespace Common.Entities;

public class Role
{
    [Key]
    public int RoleID { get; set; }
    public string RoleName { get; set; }
}
