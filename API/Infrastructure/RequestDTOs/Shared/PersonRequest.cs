using System;

namespace API.Infrastructure.RequestDTOs.Shared;

public class PersonRequest
{
    public string FName { get; set; }
    public string LName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int RoleId { get; set; }
}
