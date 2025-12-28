using System;

namespace API.Infrastructure.RequestDTOs.Shared;

public class PersonFilterRequest
{
    public string FName { get; set; }
    public string LName { get; set; }
    public string Email { get; set; }
}
