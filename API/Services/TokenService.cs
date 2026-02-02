using System;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Common.Entities;
using DotNetEnv;

namespace API.Services;

public class TokenService
{
    public string CreateToken(Person user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        int id = 0, role = 0;
        string email = "";

        switch (user)
        {
            case Student s:
                id = s.StudentID;
                role = s.RoleID;
                email = s.Email;
                break;
            case Professor p:
                id = p.ProfessorID;
                role = p.RoleID;
                email = p.Email;
                break;
            case Admin a:
                id = a.AdminID;
                role = a.RoleID;
                email = a.Email;
                break;
        }
        Claim[] claims = new Claim[]
        {
            new Claim("id",id.ToString()),
            new Claim("role",role.ToString()),
            new Claim("email",email)
        };

        Env.Load();
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret));
        var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: "ag",
            audience: "front-end",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: cred
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
