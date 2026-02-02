using System;

namespace Common.Entities;

public abstract class Person : BaseEntity
{
    public string FName { get; set; }
    public string LName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int RoleID { get; set; }


    public virtual Role Role { get; set; }
}