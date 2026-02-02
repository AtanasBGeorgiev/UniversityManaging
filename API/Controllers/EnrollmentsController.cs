using System.Linq.Expressions;
using API.Infrastructure.RequestDTOs.Enrollment;
using API.Infrastructure.RequestDTOs.Shared;
using Common;
using Common.Entities;
using Common.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EnrollmentsController : OthorizationController
    {
        [HttpGet]
        public IActionResult Get([FromBody] EnrollmentGetRequest model)
        {
            if (model == null)
                return BadRequest(ServiceResult<Enrollment>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Invalid Enrollments data."}}
                    }));

            model.Pager ??= new PagerRequest();//check if model.Pager is null

            model.Pager.Page = model.Pager.Page <= 0
            ? 1
            : model.Pager.Page;

            model.Pager.PageSize = model.Pager.PageSize <= 0
            ? 10
            : model.Pager.PageSize;

            string? key = typeof(Enrollment).GetProperties().FirstOrDefault()?.Name;
            model.OrderBy ??= key;
            model.OrderBy = typeof(Enrollment).GetProperty(model.OrderBy) != null
            ? model.OrderBy
            : key;

            model.Filter ??= new EnrollmentFilterRequest();

            EnrollmentService service = new EnrollmentService();

            Expression<Func<Enrollment, bool>> filter =
            s =>
            (model.Filter.StudentID == 0 || s.StudentID == model.Filter.StudentID) &&
            (model.Filter.CourseID == 0 || s.CourseID == model.Filter.CourseID) &&
            (string.IsNullOrEmpty(model.Filter.Year) || s.Year.Contains(model.Filter.Year)) &&
            (model.Filter.Semester == 0 || s.Semester == model.Filter.Semester) &&
            (model.Filter.Grade == 0 || s.Grade == model.Filter.Grade);

            return Ok(ServiceResult<List<Enrollment>>.Success(service.GetAll(filter, model.OrderBy, model.SortAsc, model.Pager.Page, model.Pager.PageSize)));
        }

        [HttpPost]
        [Authorize(Policy = "Admin")]
        public IActionResult Post([FromBody] EnrollmentFilterRequest model)
        {
            CourseService cService = new CourseService();
            var course = cService.GetById(model.CourseID);

            //if course exists
            if (course == null)
                return NotFound(ServiceResult<Course?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Course not found."}}
                    }));

            StudentService sService = new StudentService();
            var Student = sService.GetById(model.StudentID);

            //if Student exists
            if (Student == null)
                return NotFound(ServiceResult<Student?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Student not found."}}
                    }));

            //are course and Student from same faculty
            if (course.FacultyID != Student.FacultyID)
                return BadRequest(ServiceResult<Enrollment?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Choosen Student and course are from different faculties."}}
                    }));

            if (CheckAdminOthorization(course.FacultyID) == false)
                return BadRequest(
                       ServiceResult<Admin?>.Failure(null, new List<Error>
                       {
                            new Error
                            {
                                Key="Global",
                                Messages=new List<string>(){"You are not authorized to add enrollment in other faculties."}
                            }
                       }));

            EnrollmentService esService = new EnrollmentService();
            var check = esService.GetById(model.CourseID, model.StudentID);

            //if this pair already exists
            if (check != null)
                return BadRequest(ServiceResult<Enrollment?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"This Student has already enrolled in this course."}}
                    }));

            var item = new Enrollment
            {
                StudentID = model.StudentID,
                CourseID = model.CourseID,
                Year = model.Year,
                Semester = model.Semester
            };
            if (model.Grade != 0)
            {
                item.Grade = model.Grade;
            }

            esService.Save(item);

            return Ok(ServiceResult<Enrollment>.Success(item));
        }

        [HttpPut]
        [Authorize(Policy = "AdminOrProfessor")]
        public IActionResult Put([FromBody] EnrollmentFilterRequest model)
        {
            EnrollmentService service = new EnrollmentService();
            var enrollment = service.GetById(model.StudentID, model.CourseID);

            if (enrollment == null)
                return NotFound(ServiceResult<Student?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Enrollment not found."}}
                    }));

            if (User.FindFirst(ClaimTypes.Role)?.Value == "2")
            {
                int professorId = int.Parse(User.FindFirst("id")!.Value);

                CourseProfessorService cpService = new CourseProfessorService();
                var cp = cpService.GetById(model.CourseID, professorId);

                if (cp == null)
                {
                    return StatusCode(StatusCodes.Status403Forbidden,
                       ServiceResult<Professor?>.Failure(null, new List<Error>
                       {
                            new Error
                            {
                                Key="Global",
                                Messages=new List<string>(){"You are not authorized to modify enrollments for courses in which you are not a lecture."}
                            }
                       }
                       )
                   );
                }
                if (model.Grade != 0)
                {
                    enrollment.Grade = model.Grade;
                }
            }
            else if (User.FindFirst(ClaimTypes.Role)?.Value == "1")
            {

            }
            else
            {
                return Forbid();
            }

            CourseService cService = new CourseService();
            var course = cService.GetById(model.CourseID);

            StudentService sService = new StudentService();
            var student = sService.GetById(model.StudentID);

            if (course.FacultyID != student.FacultyID)
                return BadRequest(ServiceResult<Enrollment?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Choosen Student and course are from different faculties."}}
                    }));

            if (CheckAdminOthorization(course.FacultyID) == false)
                return BadRequest(
                       ServiceResult<Admin?>.Failure(null, new List<Error>
                       {
                            new Error
                            {
                                Key="Global",
                                Messages=new List<string>(){"You are not authorized to update enrollment in other faculties."}
                            }
                       }));

            enrollment.StudentID = model.StudentID;
            enrollment.CourseID = model.CourseID;
            enrollment.Year = model.Year;
            enrollment.Semester = model.Semester;

            service.Save(enrollment, 0, true);

            return Ok(ServiceResult<Enrollment>.Success(enrollment));
        }

        [HttpDelete]
        [Authorize(Policy = "Admin")]
        public IActionResult Delete([FromBody] EnrollmentRequest model)
        {
            if (model.CourseID == 0 || model.StudentID == 0)
            {
                return BadRequest(ServiceResult<Enrollment?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Invalid Enrollment data."}}
                    }));
            }

            EnrollmentService cpService = new EnrollmentService();
            var item = cpService.GetById(model.StudentID, model.CourseID);

            if (item == null)
                return NotFound(ServiceResult<Enrollment?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Course-Student pair not found."}}
                    }));

            CourseService cService = new CourseService();
            var course = cService.GetById(model.CourseID);

            if (CheckAdminOthorization(course.FacultyID) == false)
                return BadRequest(
                       ServiceResult<Admin?>.Failure(null, new List<Error>
                       {
                            new Error
                            {
                                Key="Global",
                                Messages=new List<string>(){"You are not authorized to delete enrollment in other faculties."}
                            }
                       }));

            cpService.Delete(item);

            return Ok(ServiceResult<Enrollment>.Success(item));
        }
    }
}