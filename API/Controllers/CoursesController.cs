using System.Linq.Expressions;
using System.Security.Claims;
using API.Infrastructure.RequestDTOs.Course;
using API.Infrastructure.RequestDTOs.Shared;
using API.Services;
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
    public class CoursesController : OthorizationController
    {
        [HttpGet]
        public IActionResult Get([FromBody] CourseGetRequest model)
        {
            if (model == null)
                return BadRequest(ServiceResult<Course?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Invalid Course data."}}
                    }));

            model.Pager ??= new PagerRequest();//check if model.Pager is null

            model.Pager.Page = model.Pager.Page <= 0
            ? 1
            : model.Pager.Page;

            model.Pager.PageSize = model.Pager.PageSize <= 0
            ? 10
            : model.Pager.PageSize;

            string? key = typeof(Course).GetProperties().FirstOrDefault()?.Name;
            model.OrderBy ??= key;
            model.OrderBy = typeof(Course).GetProperty(model.OrderBy) != null
            ? model.OrderBy
            : key;

            model.Filter ??= new CourseFilterRequest();

            CourseService service = new CourseService();

            Expression<Func<Course, bool>> filter =
            s =>
            (string.IsNullOrEmpty(model.Filter.Title) || s.Title.Contains(model.Filter.Title)) &&
            (model.Filter.Credits == 0 || s.FacultyID == model.Filter.Credits) &&
            (model.Filter.FacultyID == 0 || s.FacultyID == model.Filter.FacultyID);

            return Ok(ServiceResult<List<Course>>.Success(service.GetAll(filter, model.OrderBy, model.SortAsc, model.Pager.Page, model.Pager.PageSize)));
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult Get([FromRoute] int id)
        {
            CourseService service = new CourseService();

            var Course = service.GetById(id);

            if (Course == null)
                return NotFound(ServiceResult<Course?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Course not found."}}
                    }));

            return Ok(ServiceResult<Course>.Success(Course));
        }

        [HttpPost]
        [Route("{facultyId}")]
        [Authorize(Policy = "AdminOrProfessor")]
        public IActionResult Post([FromRoute] int facultyId, [FromBody] CourseRequest? model)
        {
            if (model == null)
                return BadRequest(ServiceResult<Course?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Invalid Course data."}
                    }}));

            CourseService service = new CourseService();

            var course = service.GetByProperty(c => c.Title == model.Title);

            if (course != null)
            {
                if (course.FacultyID == facultyId)
                    return Conflict(ServiceResult<Course?>.Failure(null, new List<Error>
                    {
                        new Error
                        {
                            Key="Global",
                            Messages=new List<string>(){"Title already exists in this faculty."}
                        }}));
            }

            FacultyService facultyService = new FacultyService();
            if (facultyService.GetById(facultyId) == null)
            {
                return NotFound(ServiceResult<Course?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Faculty not found."}
                    }}));
            }

            //if professor is logged and id tries to add enrollment in other faculty
            if (CheckProfessorOthorization(facultyId) == false || CheckAdminOthorization(facultyId) == false)
                return BadRequest(
                   ServiceResult<Professor?>.Failure(null, new List<Error>
                   {
                            new Error
                            {
                                Key="Global",
                                Messages=new List<string>(){"You are not authorized to update courses in other faculties."}
                            }
                   }
                   )
               );

            var item = new Course
            {
                Title = model.Title,
                Credits = model.Credits,
                FacultyID = facultyId
            };

            service.Save(item);

            return Ok(ServiceResult<Course>.Success(item));
        }

        [HttpPut]
        [Route("{id}")]
        [Authorize(Policy = "AdminOrProfessor")]
        public IActionResult Put([FromRoute] int id, [FromBody] CourseRequest model)
        {
            if (model == null)
                return BadRequest(ServiceResult<Course?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Invalid Course data."}
                    }}));

            FacultyService fService = new FacultyService();
            var faculty = fService.GetById(model.FacultyID);

            if (faculty == null)
                return BadRequest(ServiceResult<Course?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Faculty not found."}
                    }}));

            CourseService service = new CourseService();

            Course forUpdate = service.GetById(id);

            if (forUpdate == null)
                return NotFound(ServiceResult<Course?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Course not found."}
                    }}));

            if (forUpdate.FacultyID == model.FacultyID && forUpdate.Title == model.Title)
            {
                return Conflict(ServiceResult<Course?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Title already in use."}
                    }}));
            }

            if (CheckProfessorOthorization(forUpdate.FacultyID) == false || CheckAdminOthorization(forUpdate.FacultyID) == false)
                return BadRequest(
                   ServiceResult<Professor?>.Failure(null, new List<Error>
                   {
                            new Error
                            {
                                Key="Global",
                                Messages=new List<string>(){"You are not authorized to update courses in other faculties."}
                            }
                   }
                   )
               );

            forUpdate.Title = model.Title;
            forUpdate.Credits = model.Credits;
            if (model.FacultyID > 0)
            {
                forUpdate.FacultyID = model.FacultyID;
            }

            service.Save(forUpdate);

            return Ok(ServiceResult<Course>.Success(forUpdate));
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize(Policy = "AdminOrProfessor")]
        public IActionResult Delete([FromRoute] int id)
        {
            CourseService service = new CourseService();

            Course forDelete = service.GetById(id);

            if (forDelete == null)
                return NotFound(ServiceResult<Course?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Course not found."}
                    }}));

            if (CheckProfessorOthorization(forDelete.FacultyID) == false || CheckAdminOthorization(forDelete.FacultyID) == false)
                return BadRequest(
                   ServiceResult<Professor?>.Failure(null, new List<Error>
                   {
                            new Error
                            {
                                Key="Global",
                                Messages=new List<string>(){"You are not authorized to delete courses in other faculties."}
                            }
                   }
                   )
               );

            service.Delete(forDelete);

            return Ok(ServiceResult<Course>.Success(forDelete));
        }
    }
}