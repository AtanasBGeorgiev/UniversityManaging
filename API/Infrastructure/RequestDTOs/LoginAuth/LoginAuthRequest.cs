using System;

namespace API.Infrastructure.RequestDTOs.Login;

public class LoginAuthRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}
