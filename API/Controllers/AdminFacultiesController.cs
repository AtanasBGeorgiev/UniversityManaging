using System.Linq.Expressions;
using API.Infrastructure.RequestDTOs.AdminFaculty;
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
    [Authorize(Policy = "Admin")]
    public class AdminFacultiesController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get([FromBody] AdminFacultyGetRequest model)
        {
            if (model == null)
                return BadRequest(ServiceResult<AdminFaculty>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Invalid AdminFaculty data."}}
                    }));

            model.Pager ??= new PagerRequest();//check if model.Pager is null

            model.Pager.Page = model.Pager.Page <= 0
            ? 1
            : model.Pager.Page;

            model.Pager.PageSize = model.Pager.PageSize <= 0
            ? 10
            : model.Pager.PageSize;

            string? key = typeof(AdminFaculty).GetProperties().FirstOrDefault()?.Name;
            model.OrderBy ??= key;
            model.OrderBy = typeof(AdminFaculty).GetProperty(model.OrderBy) != null
            ? model.OrderBy
            : key;

            model.Filter ??= new AdminFacultyFilterRequest();

            AdminFacultyService service = new AdminFacultyService();

            Expression<Func<AdminFaculty, bool>> filter =
            s =>
            (model.Filter.AdminID == 0 || s.AdminID == model.Filter.AdminID) &&
            (model.Filter.FacultyID == 0 || s.FacultyID == model.Filter.FacultyID);

            return Ok(ServiceResult<List<AdminFaculty>>.Success(service.GetAll(filter, model.OrderBy, model.SortAsc, model.Pager.Page, model.Pager.PageSize)));
        }

        [HttpPost]
        public IActionResult Post([FromBody] AdminFacultyRequest model)
        {
            int AdminID = model.AdminID;
            int FacultyID = model.FacultyID;

            if (AdminID == 0 || FacultyID == 0)
            {
                return BadRequest(ServiceResult<AdminFaculty?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Invalid AdminFaculty data."}}
                    }));
            }

            AdminService aService = new AdminService();
            var admin = aService.GetById(AdminID);

            //if admin exists
            if (admin == null)
                return NotFound(ServiceResult<Course?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Admin not found."}}
                    }));

            FacultyService fService = new FacultyService();
            var faculty = fService.GetById(FacultyID);

            //if faculty exists
            if (faculty == null)
                return NotFound(ServiceResult<Professor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Faculty not found."}}
                    }));

            AdminFacultyService afService = new AdminFacultyService();
            var check = afService.GetById(AdminID, FacultyID);

            //if this pair already exists
            if (check != null)
                return BadRequest(ServiceResult<AdminFaculty?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"This pair of admin - faculty already exists."}}
                    }));

            var item = new AdminFaculty
            {
                AdminID = AdminID,
                FacultyID = FacultyID
            };

            afService.Save(item);

            return Ok(ServiceResult<AdminFaculty>.Success(item));
        }

        [HttpPut]
        public IActionResult Put([FromBody] AdminFacultyRequest model, [FromQuery] int newFacultyID)
        {
            AdminFacultyService afServiceCheck = new AdminFacultyService();

            if (afServiceCheck.GetById(model.AdminID, model.FacultyID) == null)
                return BadRequest(ServiceResult<AdminFaculty?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Old pair not found."}}
                    }));

            FacultyService fService = new FacultyService();
            var faculty = fService.GetById(newFacultyID);

            //if faculty exists
            if (faculty == null)
                return NotFound(ServiceResult<Professor?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"New faculty not found."}}
                    }));

            AdminFacultyService afService = new AdminFacultyService();

            if (afService.GetById(model.AdminID, newFacultyID) != null)
                return BadRequest(ServiceResult<AdminFaculty?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"This pair already exists."}}
                    }));

            var item = new AdminFaculty
            {
                AdminID = model.AdminID,
                FacultyID = newFacultyID
            };

            afService.Save(item, model.FacultyID);

            return Ok(ServiceResult<AdminFaculty>.Success(item));
        }

        [HttpDelete]
        public IActionResult Delete([FromBody] AdminFacultyRequest model)
        {
            AdminFacultyService afService = new AdminFacultyService();
            var item = afService.GetById(model.AdminID, model.FacultyID);

            if (item == null)
                return NotFound(ServiceResult<AdminFaculty?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Course-admin pair not found."}}
                    }));

            afService.Delete(item);

            return Ok(ServiceResult<AdminFaculty>.Success(item));
        }
    }
}