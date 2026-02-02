using System;

namespace API.Infrastructure.ResponseDTOs.Login;

public class LoginAuthResponse
{
    public string Token { get; set; }
    public int RoleID { get; set; }
    public int UserId { get; set; }
    public string Email { get; set; }
}
