using System.Linq.Expressions;
using API.Infrastructure.RequestDTOs.Faculty;
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
    public class FacultiesController : OthorizationController
    {
        [HttpGet]
        public IActionResult Get([FromBody] FacultyGetRequest model)
        {
            if (model == null)
                return BadRequest(ServiceResult<Faculty?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Invalid faculty data."}}
                    }));

            model.Pager ??= new PagerRequest();//check if model.Pager is null

            model.Pager.Page = model.Pager.Page <= 0
            ? 1
            : model.Pager.Page;

            model.Pager.PageSize = model.Pager.PageSize <= 0
            ? 10
            : model.Pager.PageSize;

            string? key = typeof(Faculty).GetProperties().FirstOrDefault()?.Name;
            model.OrderBy ??= key;
            model.OrderBy = typeof(Faculty).GetProperty(model.OrderBy) != null
            ? model.OrderBy
            : key;

            model.Filter ??= new FacultyFilterRequest();

            FacultyService service = new FacultyService();

            Expression<Func<Faculty, bool>> filter =
            f =>
            (string.IsNullOrEmpty(model.Filter.Name) || f.Name.Contains(model.Filter.Name));

            return Ok(ServiceResult<List<Faculty>>.Success(service.GetAll(filter, model.OrderBy, model.SortAsc, model.Pager.Page, model.Pager.PageSize)));
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult Get([FromRoute] int id)
        {
            FacultyService service = new FacultyService();

            var faculty = service.GetById(id);

            if (faculty == null)
                return NotFound(ServiceResult<Faculty?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Faculty not found."}}
                    }));

            return Ok(ServiceResult<Faculty>.Success(faculty));
        }

        [HttpPost]
        [Authorize(Policy = "Admin")]
        public IActionResult Post([FromBody] FacultyRequest model)
        {
            if (model == null)
                return BadRequest(ServiceResult<Faculty?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Invalid faculty data."}}
                    }));

            FacultyService service = new FacultyService();

            if (service.Count(f => f.Name == model.Name) > 0)
                return Conflict(ServiceResult<Faculty?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Faculty with the same name already exists."}}
                    }));

            var item = new Faculty
            {
                Name = model.Name
            };

            service.Save(item);

            return Ok(ServiceResult<Faculty>.Success(item));
        }

        [HttpPut]
        [Route("{id}")]
        [Authorize(Policy = "Admin")]
        public IActionResult Put([FromRoute] int id, [FromBody] FacultyRequest model)
        {
            if (model == null)
                return BadRequest(ServiceResult<Faculty?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Invalid faculty data."}}
                    }));

            FacultyService service = new FacultyService();

            Faculty forUpdate = service.GetById(id);

            if (forUpdate == null)
                return NotFound(ServiceResult<Faculty?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Faculty not found."}}
                    }));

            if (forUpdate.Name != model.Name && service.Count(f => f.Name == model.Name) > 0)
            {
                return Conflict(ServiceResult<Faculty?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Faculty with the same name already exists."}}
                    }));
            }

            if (CheckAdminOthorization(id) == false)
                return BadRequest(
                       ServiceResult<Admin?>.Failure(null, new List<Error>
                       {
                            new Error
                            {
                                Key="Global",
                                Messages=new List<string>(){"You are not authorized to update other faculties."}
                            }
                       }));

            forUpdate.Name = model.Name;

            service.Save(forUpdate);

            return Ok(ServiceResult<Faculty>.Success(forUpdate));
        }

        //add dean
        [HttpPut]
        [Authorize(Policy = "Admin")]
        public IActionResult Put([FromQuery] int facultyID, [FromQuery] int professorID)
        {
            if (facultyID == null || professorID == null)
                return BadRequest(ServiceResult<Faculty?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Invalid faculty/professor data."}}
                    }));

            FacultyService service = new FacultyService();

            Faculty updateFaculty = service.GetById(facultyID);

            if (updateFaculty == null)
                return NotFound(ServiceResult<Faculty?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Faculty not found."}}
                    }));

            if (CheckAdminOthorization(facultyID) == false)
                return BadRequest(
                       ServiceResult<Admin?>.Failure(null, new List<Error>
                       {
                            new Error
                            {
                                Key="Global",
                                Messages=new List<string>(){"You are not authorized to update other faculties."}
                            }
                       }));

            ProfessorService profService = new ProfessorService();
            Professor professor = profService.GetById(professorID);

            if (professor == null)
            {
                return NotFound(ServiceResult<Professor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Professor not found."}}
                    }));
            }

            if (professor.FacultyID != facultyID)
            {
                return BadRequest(ServiceResult<Professor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"The choosen professor is from another faculty."}}
                    }));
            }

            updateFaculty.DeanID = professorID;

            service.Save(updateFaculty);

            return Ok(ServiceResult<Faculty>.Success(updateFaculty));
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize(Policy = "Admin")]
        public IActionResult Delete([FromRoute] int id)
        {
            FacultyService service = new FacultyService();

            Faculty forDelete = service.GetById(id);

            if (forDelete == null)
                return NotFound(ServiceResult<Faculty?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Faculty not found."}}
                    }));

            if (CheckAdminOthorization(id) == false)
                return BadRequest(
                       ServiceResult<Admin?>.Failure(null, new List<Error>
                       {
                            new Error
                            {
                                Key="Global",
                                Messages=new List<string>(){"You are not authorized to delete other faculties."}
                            }
                       }));


            var studentService = new StudentService();
            if (studentService.Count(s => s.FacultyID == forDelete.FacultyID) > 0)
            {
                return Conflict(ServiceResult<Faculty?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Cannot delete faculty with assigned students."}}
                    }));
            }

            var professorService = new ProfessorService();
            if (professorService.Count(p => p.FacultyID == forDelete.FacultyID) > 0)
            {
                return Conflict(ServiceResult<Faculty?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Cannot delete faculty with assigned professors."}}
                    }));
            }

            service.Delete(forDelete);

            return Ok(ServiceResult<Faculty>.Success(forDelete));
        }
    }
}