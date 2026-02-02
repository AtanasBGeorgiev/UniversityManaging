using API.Infrastructure.RequestDTOs.Login;
using API.Services;
using Common.Entities;
using Common;
using Microsoft.AspNetCore.Mvc;
using Common.Services;
using API.Infrastructure.ResponseDTOs.Login;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        [HttpPost]
        public IActionResult Login([FromBody] LoginAuthRequest model)
        {
            if (model == null)
                return BadRequest(ServiceResult<LoginAuthRequest?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Invalid login data."}
                    }}));

            var user = null as Person;
            int id = 0;

            StudentService studentService = new StudentService();
            if (studentService.Count(s => s.Email == model.Email) > 0)
            {
                user = studentService.GetByProperty(s => s.Email == model.Email);
                id = user.GetIds()[0];
            }
            else
            {
                ProfessorService professorService = new ProfessorService();
                if (professorService.Count(p => p.Email == model.Email) > 0)
                {
                    user = professorService.GetByProperty(p => p.Email == model.Email);
                    id = user.GetIds()[0];
                }
                else
                {
                    AdminService adminService = new AdminService();
                    if (adminService.Count(a => a.Email == model.Email) > 0)
                    {
                        user = adminService.GetByProperty(a => a.Email == model.Email);
                        id = user.GetIds()[0];
                    }
                    else
                    {
                        return BadRequest(ServiceResult<LoginAuthResponse?>.Failure(null, new List<Error>
                {
                    new Error { Key = "Global", Messages = new List<string> { "User not found." } }
                }));
                    }
                }
            }

            //reads salt and cost from hashed password
            //compares them with the new hashed entered password
            bool verified = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);

            if (!verified)
            {
                return BadRequest(ServiceResult<LoginAuthResponse?>.Failure(null, new List<Error>
                {
                    new Error { Key = "Global", Messages = new List<string> { "Invalid password." } }
                }));
            }

            var token = new TokenService().CreateToken(user);//token to string

            var response = new LoginAuthResponse
            {
                Token = token,
                RoleID = user.RoleID,
                UserId = id,
                Email = user.Email
            };

            return Ok(ServiceResult<LoginAuthResponse>.Success(response));
        }
    }
}