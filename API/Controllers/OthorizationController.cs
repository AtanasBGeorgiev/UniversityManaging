using System.Security.Claims;
using Common.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OthorizationController : ControllerBase
    {
        protected bool CheckProfessorOthorization(int facultyID)
        {
            if (User.FindFirst(ClaimTypes.Role)?.Value == "2")
            {
                int professorId = int.Parse(User.FindFirst("id")!.Value);

                ProfessorService pService = new ProfessorService();
                var professor = pService.GetById(professorId);

                if (professor.FacultyID != facultyID)
                {
                    return false;
                }
            }

            return true;
        }
        protected bool CheckAdminOthorization(int facultyID)
        {
            if (User.FindFirst(ClaimTypes.Role)?.Value == "1")
            {
                int adminId = int.Parse(User.FindFirst("id")!.Value);

                AdminFacultyService afService = new AdminFacultyService();
                var pair = afService.GetById(adminId, facultyID);

                if (pair == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
