using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using DotNetEnv;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Common.Persistance;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

Env.Load();
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),//checks original and used secret key
            ValidIssuer = "ag",
            ValidAudience = "front-end",
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", p => p.RequireClaim(ClaimTypes.Role, "1"));
    options.AddPolicy("Professor", p => p.RequireClaim(ClaimTypes.Role, "2"));
    options.AddPolicy("Student", p => p.RequireClaim(ClaimTypes.Role, "3"));
    options.AddPolicy("AdminOrProfessor", p => p.RequireClaim(ClaimTypes.Role, "1", "2"));
    options.AddPolicy("AdminOrStudent", p => p.RequireClaim(ClaimTypes.Role, "1", "3"));
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

/*builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});*/

var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
//app.UseCors("AllowFrontend");

app.Run();