using System.Linq.Expressions;
using API.Infrastructure;
using API.Infrastructure.RequestDTOs.Admin;
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
    public class AdminsController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get([FromBody] AdminGetRequest model)
        {
            if (model == null)
                return BadRequest(ServiceResult<Admin?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Invalid Admin data."}}
                    }));

            model.Pager ??= new PagerRequest();//check if model.Pager is null

            model.Pager.Page = model.Pager.Page <= 0
            ? 1
            : model.Pager.Page;

            model.Pager.PageSize = model.Pager.PageSize <= 0
            ? 10
            : model.Pager.PageSize;

            string? key = typeof(Admin).GetProperties().FirstOrDefault()?.Name;
            model.OrderBy ??= key;
            model.OrderBy = typeof(Admin).GetProperty(model.OrderBy) != null
            ? model.OrderBy
            : key;

            model.Filter ??= new AdminFilterRequest();

            AdminService service = new AdminService();

            Expression<Func<Admin, bool>> filter =
            s =>
            (string.IsNullOrEmpty(model.Filter.FName) || s.FName.Contains(model.Filter.FName)) &&
            (string.IsNullOrEmpty(model.Filter.LName) || s.LName.Contains(model.Filter.LName)) &&
            (string.IsNullOrEmpty(model.Filter.Email) || s.Email.Contains(model.Filter.Email));

            return Ok(ServiceResult<List<Admin>>.Success(service.GetAll(filter, model.OrderBy, model.SortAsc, model.Pager.Page, model.Pager.PageSize)));
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize(Policy = "Admin")]
        public IActionResult Get([FromRoute] int id)
        {
            AdminService service = new AdminService();

            var Admin = service.GetById(id);

            if (Admin == null)
                return NotFound(ServiceResult<Admin?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Admin not found."}}
                    }));

            return Ok(ServiceResult<Admin>.Success(Admin));
        }

        [HttpPost]
        public IActionResult Post([FromBody] AdminRequest? model)
        {
            if (model == null)
                return BadRequest(ServiceResult<Admin?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Invalid Admin data."}
                    }}));

            AdminService service = new AdminService();

            if (service.Count(s => s.Email == model.Email) > 0)
            {
                return Conflict(ServiceResult<Admin?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Email already in use."}
                    }}));
            }

            var item = new Admin
            {
                FName = model.FName,
                LName = model.LName,
                Email = model.Email,
                Password = HashPassword.Hash(model.Password),
                RoleID = 1
            };

            service.Save(item);

            return Ok(ServiceResult<Admin>.Success(item));
        }

        [HttpPut]
        [Route("{id}")]
        public IActionResult Put([FromRoute] int id, [FromBody] AdminRequest model)
        {
            if (model == null)
                return BadRequest(ServiceResult<Admin?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Invalid Admin data."}
                    }}));

            AdminService service = new AdminService();

            Admin forUpdate = service.GetById(id);

            if (forUpdate == null)
                return NotFound(ServiceResult<Admin?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Admin not found."}
                    }}));

            if (forUpdate.Email != model.Email && service.Count(s => s.Email == model.Email) > 0)
            {
                return Conflict(ServiceResult<Admin?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Email already in use."}
                    }}));
            }

            forUpdate.FName = model.FName;
            forUpdate.LName = model.LName;
            forUpdate.Email = model.Email;
            forUpdate.Password = HashPassword.Hash(model.Password);

            service.Save(forUpdate);

            return Ok(ServiceResult<Admin>.Success(forUpdate));
        }

        [HttpDelete]
        [Route("{id}")]
        public IActionResult Delete([FromRoute] int id)
        {
            AdminService service = new AdminService();

            Admin forDelete = service.GetById(id);

            if (forDelete == null)
                return NotFound(ServiceResult<Admin?>.Failure(null, new List<Error>
                {
                    new Error
                    {
                        Key="Global",
                        Messages=new List<string>(){"Admin not found."}
                    }}));

            service.Delete(forDelete);

            return Ok(ServiceResult<Admin>.Success(forDelete));
        }
    }
}