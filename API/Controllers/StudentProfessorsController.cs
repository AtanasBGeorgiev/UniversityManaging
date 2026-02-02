using System.Linq.Expressions;
using API.Infrastructure.RequestDTOs.Shared;
using API.Infrastructure.RequestDTOs.StudentProfessors;
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
    public class StudentProfessorsController : OthorizationController
    {
        [HttpGet]
        public IActionResult Get([FromBody] StudentProfessorsGetRequest model)
        {
            if (model == null)
                return BadRequest(ServiceResult<StudentProfessor>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Invalid StudentProfessors data."}}
                    }));

            model.Pager ??= new PagerRequest();//check if model.Pager is null

            model.Pager.Page = model.Pager.Page <= 0
            ? 1
            : model.Pager.Page;

            model.Pager.PageSize = model.Pager.PageSize <= 0
            ? 10
            : model.Pager.PageSize;

            string? key = typeof(StudentProfessor).GetProperties().FirstOrDefault()?.Name;
            model.OrderBy ??= key;
            model.OrderBy = typeof(StudentProfessor).GetProperty(model.OrderBy) != null
            ? model.OrderBy
            : key;

            model.Filter ??= new StudentProfessorsFilterRequest();

            StudentProfessorService service = new StudentProfessorService();

            Expression<Func<StudentProfessor, bool>> filter =
            s =>
            (model.Filter.StudentID == 0 || s.StudentID == model.Filter.StudentID) &&
            (model.Filter.ProfessorID == 0 || s.ProfessorID == model.Filter.ProfessorID);

            return Ok(ServiceResult<List<StudentProfessor>>.Success(service.GetAll(filter, model.OrderBy, model.SortAsc, model.Pager.Page, model.Pager.PageSize)));
        }

        [HttpPost]
        [Authorize(Policy = "Admin")]
        public IActionResult Post([FromBody] StudentProfessorsRequest model)
        {
            int StudentID = model.StudentID;
            int ProfessorID = model.ProfessorID;

            if (StudentID == 0 || ProfessorID == 0)
            {
                return BadRequest(ServiceResult<StudentProfessor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Invalid StudentProfessors data."}}
                    }));
            }

            StudentService sService = new StudentService();
            var student = sService.GetById(StudentID);

            //if student exists
            if (student == null)
                return NotFound(ServiceResult<Course?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Student not found."}}
                    }));

            ProfessorService pService = new ProfessorService();
            var professor = pService.GetById(ProfessorID);

            //if professor exists
            if (professor == null)
                return NotFound(ServiceResult<Professor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Professor not found."}}
                    }));

            if (student.FacultyID != professor.FacultyID)
                return BadRequest(ServiceResult<StudentProfessor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Student and professor are from different faculties."}}
                    }));

            if (CheckAdminOthorization(student.FacultyID) == false)
                return BadRequest(
                       ServiceResult<Admin?>.Failure(null, new List<Error>
                       {
                            new Error
                            {
                                Key="Global",
                                Messages=new List<string>(){"You are not authorized to add pair in other faculties."}
                            }
                       }));

            StudentProfessorService spService = new StudentProfessorService();
            var check = spService.GetById(StudentID, ProfessorID);

            //if this pair already exists
            if (check != null)
                return BadRequest(ServiceResult<StudentProfessor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"This pair of student - professor already exists."}}
                    }));

            var item = new StudentProfessor
            {
                StudentID = StudentID,
                ProfessorID = ProfessorID
            };

            spService.Save(item);

            return Ok(ServiceResult<StudentProfessor>.Success(item));
        }

        [HttpPut]
        [Authorize(Policy = "Admin")]
        public IActionResult Put([FromBody] StudentProfessorsRequest model, [FromQuery] int newProfessorID)
        {
            StudentProfessorService cpServiceCheck = new StudentProfessorService();

            if (cpServiceCheck.GetById(model.StudentID, model.ProfessorID) == null)
                return BadRequest(ServiceResult<StudentProfessor?>.Failure(null, new List<Error>
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

            StudentService sService = new StudentService();
            var student = sService.GetById(model.StudentID);

            if (student.FacultyID != professor.FacultyID)
                return BadRequest(ServiceResult<StudentProfessor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Student and professor are from different faculties."}}
                    }));

            if (CheckAdminOthorization(student.FacultyID) == false)
                return BadRequest(
                       ServiceResult<Admin?>.Failure(null, new List<Error>
                       {
                            new Error
                            {
                                Key="Global",
                                Messages=new List<string>(){"You are not authorized to update pair in other faculties."}
                            }
                       }));

            StudentProfessorService spService = new StudentProfessorService();

            if (spService.GetById(model.StudentID, newProfessorID) != null)
                return BadRequest(ServiceResult<StudentProfessor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"This pair already exists."}}
                    }));

            var item = new StudentProfessor
            {
                StudentID = model.StudentID,
                ProfessorID = newProfessorID
            };

            spService.Save(item, model.ProfessorID);

            return Ok(ServiceResult<StudentProfessor>.Success(item));
        }

        [HttpDelete]
        [Authorize(Policy = "Admin")]
        public IActionResult Delete([FromBody] StudentProfessorsRequest model)
        {
            StudentProfessorService spService = new StudentProfessorService();
            var item = spService.GetById(model.StudentID, model.ProfessorID);

            if (item == null)
                return NotFound(ServiceResult<StudentProfessor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Student-professor pair not found."}}
                    }));

            StudentService sService = new StudentService();
            var student = sService.GetById(model.StudentID);

            if (CheckAdminOthorization(student.FacultyID) == false)
                return BadRequest(
                       ServiceResult<Admin?>.Failure(null, new List<Error>
                       {
                            new Error
                            {
                                Key="Global",
                                Messages=new List<string>(){"You are not authorized to delete pair in other faculties."}
                            }
                       }));

            spService.Delete(item);

            return Ok(ServiceResult<StudentProfessor>.Success(item));
        }
    }
}