using System.Linq.Expressions;
using System.Security.Claims;
using API.Infrastructure;
using API.Infrastructure.RequestDTOs;
using API.Infrastructure.RequestDTOs.Shared;
using API.Infrastructure.RequestDTOs.Student;
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
    public class StudentsController : OthorizationController
    {
        [HttpGet]
        public IActionResult Get([FromBody] StudentsGetRequest model)
        {
            if (model == null)
                return BadRequest(ServiceResult<Student?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Invalid student data."}}
                    }));

            model.Pager ??= new PagerRequest();//check if model.Pager is null

            model.Pager.Page = model.Pager.Page <= 0
            ? 1
            : model.Pager.Page;

            model.Pager.PageSize = model.Pager.PageSize <= 0
            ? 10
            : model.Pager.PageSize;

            string? key = typeof(Student).GetProperties().FirstOrDefault()?.Name;//takes first pubic atribute
            model.OrderBy ??= key;//set it if OrderBy is null
            model.OrderBy = typeof(Student).GetProperty(model.OrderBy) != null//if OrderBy is not null then check if this attribute exists
            ? model.OrderBy
            : key;

            model.Filter ??= new StudentFilterRequest();

            StudentService service = new StudentService();

            Expression<Func<Student, bool>> filter =
            s =>
            (string.IsNullOrEmpty(model.Filter.FName) || s.FName.Contains(model.Filter.FName)) &&
            (string.IsNullOrEmpty(model.Filter.LName) || s.LName.Contains(model.Filter.LName)) &&
            (string.IsNullOrEmpty(model.Filter.Email) || s.Email.Contains(model.Filter.Email)) &&
            (model.Filter.FacultyId == 0 || s.FacultyID == model.Filter.FacultyId);

            return Ok(ServiceResult<List<Student>>.Success(service.GetAll(filter, model.OrderBy, model.SortAsc, model.Pager.Page, model.Pager.PageSize)));
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult Get([FromRoute] int id)
        {
            StudentService service = new StudentService();

            var student = service.GetById(id);

            if (student == null)
                return NotFound(ServiceResult<Student?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Student not found."}}
                    }));

            return Ok(ServiceResult<Student>.Success(student));
        }

        [HttpPost]
        [Route("{facultyId}")]
        [Authorize(Policy = "Admin")]
        public IActionResult Post([FromRoute] int facultyId, [FromBody] StudentRequest? model)
        {
            if (model == null)
                return BadRequest(ServiceResult<Student?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Invalid student data."}
                    }}));

            StudentService service = new StudentService();

            if (service.Count(s => s.Email == model.Email) > 0)
            {
                return Conflict(ServiceResult<Student?>.Failure(null, new List<Error>
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
                return NotFound(ServiceResult<Student?>.Failure(null, new List<Error>
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
                                Messages=new List<string>(){"You are not authorized to add students from other faculties."}
                            }
                       }));

            var item = new Student
            {
                FName = model.FName,
                LName = model.LName,
                Email = model.Email,
                Password = HashPassword.Hash(model.Password),
                RoleID = 3,
                FacultyID = facultyId
            };

            service.Save(item);

            return Ok(ServiceResult<Student>.Success(item));
        }

        [HttpPut]
        [Route("{id}")]
        [Authorize(Policy = "AdminOrStudent")]
        public IActionResult Put([FromRoute] int id, [FromBody] StudentRequest model)
        {
            if (model == null)
                return BadRequest(ServiceResult<Student?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Invalid student data."}
                    }}));

            StudentService service = new StudentService();

            Student forUpdate = service.GetById(id);

            if (forUpdate == null)
                return NotFound(ServiceResult<Student?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Student not found."}
                    }}));

            if (forUpdate.Email != model.Email && service.Count(s => s.Email == model.Email) > 0)
            {
                return Conflict(ServiceResult<Student?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Email already in use."}
                    }}));
            }

            if (User.FindFirst(ClaimTypes.Role)?.Value == "3" && id != int.Parse(User.FindFirst("id")!.Value))
                return BadRequest(ServiceResult<Student?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"You cannot change data of another student."}
                    }}));

            if (CheckAdminOthorization(forUpdate.FacultyID) == false)
                return BadRequest(
                       ServiceResult<Admin?>.Failure(null, new List<Error>
                       {
                            new Error
                            {
                                Key="Global",
                                Messages=new List<string>(){"You are not authorized to uodate students from other faculties."}
                            }
                       }));

            forUpdate.FName = model.FName;
            forUpdate.LName = model.LName;
            forUpdate.Email = model.Email;
            forUpdate.Password = HashPassword.Hash(model.Password);

            service.Save(forUpdate);

            return Ok(ServiceResult<Student>.Success(forUpdate));
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize(Policy = "Admin")]
        public IActionResult Delete([FromRoute] int id)
        {
            StudentService service = new StudentService();

            Student forDelete = service.GetById(id);

            if (forDelete == null)
                return NotFound(ServiceResult<Student?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Student not found."}
                    }}));

            if (CheckAdminOthorization(forDelete.FacultyID) == false)
                return BadRequest(
                       ServiceResult<Admin?>.Failure(null, new List<Error>
                       {
                            new Error
                            {
                                Key="Global",
                                Messages=new List<string>(){"You are not authorized to uodate students from other faculties."}
                            }
                       }));

            service.Delete(forDelete);

            return Ok(ServiceResult<Student>.Success(forDelete));
        }
    }
}
