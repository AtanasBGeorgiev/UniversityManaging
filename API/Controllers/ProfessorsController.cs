using System.Linq.Expressions;
using System.Security.Claims;
using API.Infrastructure;
using API.Infrastructure.RequestDTOs;
using API.Infrastructure.RequestDTOs.Professor;
using API.Infrastructure.RequestDTOs.Shared;
using Common;
using Common.Entities;
using Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfessorsController : OthorizationController
    {
        [HttpGet]
        public IActionResult Get([FromBody] ProfessorGetRequest model)
        {
            if (model == null)
                return BadRequest(ServiceResult<Professor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Invalid Professor data."}}
                    }));

            model.Pager ??= new PagerRequest();//check if model.Pager is null

            model.Pager.Page = model.Pager.Page <= 0
            ? 1
            : model.Pager.Page;

            model.Pager.PageSize = model.Pager.PageSize <= 0
            ? 10
            : model.Pager.PageSize;

            string? key = typeof(Professor).GetProperties().FirstOrDefault()?.Name;
            model.OrderBy ??= key;
            model.OrderBy = typeof(Professor).GetProperty(model.OrderBy) != null
            ? model.OrderBy
            : key;

            model.Filter ??= new ProfessorFilterRequest();

            ProfessorService service = new ProfessorService();

            Expression<Func<Professor, bool>> filter =
            s =>
            (string.IsNullOrEmpty(model.Filter.FName) || s.FName.Contains(model.Filter.FName)) &&
            (string.IsNullOrEmpty(model.Filter.LName) || s.LName.Contains(model.Filter.LName)) &&
            (string.IsNullOrEmpty(model.Filter.Email) || s.Email.Contains(model.Filter.Email)) &&
            (model.Filter.FacultyId == 0 || s.FacultyID == model.Filter.FacultyId);

            return Ok(ServiceResult<List<Professor>>.Success(service.GetAll(filter, model.OrderBy, model.SortAsc, model.Pager.Page, model.Pager.PageSize)));
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult Get([FromRoute] int id)
        {
            ProfessorService service = new ProfessorService();

            var Professor = service.GetById(id);

            if (Professor == null)
                return NotFound(ServiceResult<Professor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Professor not found."}}
                    }));

            return Ok(ServiceResult<Professor>.Success(Professor));
        }

        [HttpPost]
        [Route("{facultyId}")]
        [Authorize(Policy = "Admin")]
        public IActionResult Post([FromRoute] int facultyId, [FromBody] ProfessorRequest? model)
        {
            if (model == null)
                return BadRequest(ServiceResult<Professor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Invalid Professor data."}
                    }}));

            ProfessorService service = new ProfessorService();

            if (service.Count(s => s.Email == model.Email) > 0)
            {
                return Conflict(ServiceResult<Professor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Email already in use."}
                    }}));
            }

            FacultyService facultyService = new FacultyService();
            if (facultyService.GetById(facultyId) == null)
            {
                return NotFound(ServiceResult<Professor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Faculty not found."}
                    }}));
            }

            if (CheckAdminOthorization(facultyId) == false)
                return BadRequest(
                       ServiceResult<Admin?>.Failure(null, new List<Error>
                       {
                            new Error
                            {
                                Key="Global",
                                Messages=new List<string>(){"You are not authorized to add professor from other faculties."}
                            }
                       }));

            var item = new Professor
            {
                FName = model.FName,
                LName = model.LName,
                Email = model.Email,
                Password = HashPassword.Hash(model.Password),
                RoleID = 2,
                FacultyID = facultyId
            };

            service.Save(item);

            return Ok(ServiceResult<Professor>.Success(item));
        }

        [HttpPut]
        [Route("{id}")]
        [Authorize(Policy = "AdminOrProfessor")]
        public IActionResult Put([FromRoute] int id, [FromBody] ProfessorRequest model)
        {
            if (model == null)
                return BadRequest(ServiceResult<Professor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Invalid Professor data."}
                    }}));

            ProfessorService service = new ProfessorService();

            Professor forUpdate = service.GetById(id);

            if (forUpdate == null)
                return NotFound(ServiceResult<Professor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Professor not found."}
                    }}));

            if (forUpdate.Email != model.Email && service.Count(s => s.Email == model.Email) > 0)
            {
                return Conflict(ServiceResult<Professor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Email already in use."}
                    }}));
            }

            if (CheckAdminOthorization(forUpdate.FacultyID) == false)
                return BadRequest(
                       ServiceResult<Admin?>.Failure(null, new List<Error>
                       {
                            new Error
                            {
                                Key="Global",
                                Messages=new List<string>(){"You are not authorized to update professor from other faculties."}
                            }
                       }));

            if (User.FindFirst(ClaimTypes.Role)?.Value == "2" && id == int.Parse(User.FindFirst("id")!.Value))
            {
                forUpdate.Email = model.Email;
                forUpdate.Password = HashPassword.Hash(model.Password);
            }
            else if (User.FindFirst(ClaimTypes.Role)?.Value == "1")
            {
                forUpdate.FName = model.FName;
                forUpdate.LName = model.LName;
                forUpdate.Email = model.Email;
                forUpdate.Password = HashPassword.Hash(model.Password);
            }
            else
            {
                return Forbid();
            }

            service.Save(forUpdate);

            return Ok(ServiceResult<Professor>.Success(forUpdate));
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize(Policy = "Admin")]
        public IActionResult Delete([FromRoute] int id)
        {
            ProfessorService service = new ProfessorService();

            Professor forDelete = service.GetById(id);

            if (forDelete == null)
                return NotFound(ServiceResult<Professor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Professor not found."}
                    }}));

            if (CheckAdminOthorization(forDelete.FacultyID) == false)
                return BadRequest(
                       ServiceResult<Admin?>.Failure(null, new List<Error>
                       {
                            new Error
                            {
                                Key="Global",
                                Messages=new List<string>(){"You are not authorized to delete professor from other faculties."}
                            }
                       }));

            service.Delete(forDelete);

            return Ok(ServiceResult<Professor>.Success(forDelete));
        }
    }
}
