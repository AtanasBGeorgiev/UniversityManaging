using System.Linq.Expressions;
using System.Security.Claims;
using API.Infrastructure.RequestDTOs.CourseProfessors;
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
    public class CourseProfessorsController : OthorizationController
    {
        [HttpGet]
        public IActionResult Get([FromBody] CourseProfessorsGetRequest model)
        {
            if (model == null)
                return BadRequest(ServiceResult<CourseProfessor>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Invalid CourseProfessors data."}}
                    }));

            model.Pager ??= new PagerRequest();//check if model.Pager is null

            model.Pager.Page = model.Pager.Page <= 0
            ? 1
            : model.Pager.Page;

            model.Pager.PageSize = model.Pager.PageSize <= 0
            ? 10
            : model.Pager.PageSize;

            string? key = typeof(CourseProfessor).GetProperties().FirstOrDefault()?.Name;
            model.OrderBy ??= key;
            model.OrderBy = typeof(CourseProfessor).GetProperty(model.OrderBy) != null
            ? model.OrderBy
            : key;

            model.Filter ??= new CourseProfessorsFilterRequest();

            CourseProfessorService service = new CourseProfessorService();

            Expression<Func<CourseProfessor, bool>> filter =
            s =>
            (model.Filter.CourseID == 0 || s.CourseID == model.Filter.CourseID) &&
            (model.Filter.ProfessorID == 0 || s.ProfessorID == model.Filter.ProfessorID);

            return Ok(ServiceResult<List<CourseProfessor>>.Success(service.GetAll(filter, model.OrderBy, model.SortAsc, model.Pager.Page, model.Pager.PageSize)));
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrProfessor")]
        public IActionResult Post([FromBody] CourseProfessorsRequest model)
        {
            int courseID = model.CourseID;
            int professorID = model.ProfessorID;

            if (courseID == 0 || professorID == 0)
            {
                return BadRequest(ServiceResult<CourseProfessor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Invalid CourseProfessor data."}}
                    }));
            }

            CourseService cService = new CourseService();
            var course = cService.GetById(courseID);

            //if course exists
            if (course == null)
                return NotFound(ServiceResult<Course?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Course not found."}}
                    }));

            ProfessorService pService = new ProfessorService();
            var professor = pService.GetById(professorID);

            //if professor exists
            if (professor == null)
                return NotFound(ServiceResult<Professor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Professor not found."}}
                    }));

            if (CheckProfessorOthorization(course.FacultyID) == false)
                return BadRequest(ServiceResult<CourseProfessor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"You can not add courses from another faculties."}}
                    }));

            //are course and professor from same faculty
            if (course.FacultyID != professor.FacultyID)
                return BadRequest(ServiceResult<CourseProfessor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Choosen professor and course are from different faculties."}}
                    }));

            if (CheckAdminOthorization(course.FacultyID) == false)
                return BadRequest(
                       ServiceResult<Admin?>.Failure(null, new List<Error>
                       {
                            new Error
                            {
                                Key="Global",
                                Messages=new List<string>(){"You are not authorized to add pair in other faculties."}
                            }
                       }));

            CourseProfessorService cpService = new CourseProfessorService();
            var check = cpService.GetById(courseID, professorID);

            //if this pair already exists
            if (check != null)
                return BadRequest(ServiceResult<CourseProfessor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"This professor already owns this course."}}
                    }));

            var item = new CourseProfessor
            {
                CourseID = courseID,
                ProfessorID = professorID
            };

            cpService.Save(item);

            return Ok(ServiceResult<CourseProfessor>.Success(item));
        }

        [HttpPut]
        [Authorize(Policy = "AdminOrProfessor")]
        public IActionResult Put([FromBody] CourseProfessorsRequest model, [FromQuery] int newProfessorID)
        {
            CourseProfessorService cpServiceCheck = new CourseProfessorService();

            if (cpServiceCheck.GetById(model.CourseID, model.ProfessorID) == null)
                return BadRequest(ServiceResult<CourseProfessor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Old pair not found."}}
                    }));

            ProfessorService pService = new ProfessorService();
            var professor = pService.GetById(newProfessorID);

            //if professor exists
            if (professor == null)
                return NotFound(ServiceResult<Professor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"New professor not found."}}
                    }));

            CourseService cService = new CourseService();
            var course = cService.GetById(model.CourseID);
            //are course and professor from same faculty
            if (course.FacultyID != professor.FacultyID)
                return BadRequest(ServiceResult<CourseProfessor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Choosen professor and course are from different faculties."}}
                    }));

            if (CheckProfessorOthorization(course.FacultyID) == false)
                return BadRequest(ServiceResult<CourseProfessor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"You can not update courses from another faculties."}}
                    }));


            if (CheckAdminOthorization(course.FacultyID) == false)
                return BadRequest(
                       ServiceResult<Admin?>.Failure(null, new List<Error>
                       {
                            new Error
                            {
                                Key="Global",
                                Messages=new List<string>(){"You are not authorized to update pair in other faculties."}
                            }
                       }));

            CourseProfessorService cpService = new CourseProfessorService();

            if (cpService.GetById(model.CourseID, newProfessorID) != null)
                return BadRequest(ServiceResult<CourseProfessor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"This pair already exists."}}
                    }));

            var item = new CourseProfessor
            {
                CourseID = model.CourseID,
                ProfessorID = newProfessorID
            };

            cpService.Save(item, model.ProfessorID);

            return Ok(ServiceResult<CourseProfessor>.Success(item));
        }

        [HttpDelete]
        [Authorize(Policy = "AdminOrProfessor")]
        public IActionResult Delete([FromBody] CourseProfessorsRequest model)
        {
            CourseProfessorService cpService = new CourseProfessorService();
            var item = cpService.GetById(model.CourseID, model.ProfessorID);

            if (item == null)
                return NotFound(ServiceResult<CourseProfessor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Course-professor pair not found."}}
                    }));

            //if professor try to delete other lecture's course
            if (User.FindFirst(ClaimTypes.Role)?.Value == "2")
            {
                int professorId = int.Parse(User.FindFirst("id")!.Value);

                ProfessorService pService = new ProfessorService();
                var professor = pService.GetById(professorId);

                if (professor.ProfessorID != item.ProfessorID)
                {
                    return BadRequest(ServiceResult<CourseProfessor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"You can not delete courses in which you are not a lecture."}}
                    }));
                }
            }

            CourseService cService = new CourseService();
            var course = cService.GetById(model.CourseID);

            if (CheckAdminOthorization(course.FacultyID) == false)
                return BadRequest(
                       ServiceResult<Admin?>.Failure(null, new List<Error>
                       {
                            new Error
                            {
                                Key="Global",
                                Messages=new List<string>(){"You are not authorized to delete pair in other faculties."}
                            }
                       }));

            cpService.Delete(item);

            return Ok(ServiceResult<CourseProfessor>.Success(item));
        }
    }
}