using System;

namespace Common.Entities;

public class Person
{
    public string FName { get; set; }
    public string LName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int RoleID { get; set; }


    public virtual Role Role { get; set; }
}