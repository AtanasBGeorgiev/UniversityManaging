using System;
using System.ComponentModel.DataAnnotations;

namespace Common.Entities;

public class Role : BaseEntity
{
    [Key]
    public int RoleID { get; set; }
    public string RoleName { get; set; }

    public override int[] GetIds()
    {
        return [RoleID];
    }
}
