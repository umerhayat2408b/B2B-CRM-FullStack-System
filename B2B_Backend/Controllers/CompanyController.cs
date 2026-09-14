using B2B_PRO.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BCrypt.Net;
using System.Security.Claims;

namespace B2B_PRO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    public class CompanyController : ControllerBase
    {
        private string CurrentRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        }

        private int CurrentCompany()
        {
            return Convert.ToInt32(
                User.FindFirst("CompanyId")?.Value
            );
        }
        private readonly AppDbContext _context;
        public CompanyController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> CompanyNames()
        {
            if (CurrentRole() == "SuperAdmin")
            {
                return Ok(await _context.companyName.ToListAsync());
            }

            return Ok(await _context.companyName
                .Where(x => x.Id == CurrentCompany())
                .ToListAsync());
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> CompanayNbyname(int id)
        {

            var names = await _context.companyName.FindAsync(id);
            if (CurrentRole() != "SuperAdmin")
            {
                if (names.Id != CurrentCompany())
                {
                    return Unauthorized(new
                    {
                        message = "Access Denied"
                    });
                }
            }
            if (names == null)
            {
                return NotFound(new { message = "This id is not found" });
            }
            return Ok(names);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Createcompany([FromBody] CompanyRegisterModel model)
        {
            if (model == null)
                return BadRequest();

            var company = new CompanyN
            {
                Name = model.Name,
                CompanyName = model.CompanyName,
                Email = model.Email
            };

            _context.companyName.Add(company);

            await _context.SaveChangesAsync();

            var tenantAdmin = new User
            {
                Name = model.AdminName,
                Email = model.AdminEmail,
                Password = model.AdminPassword,
                Role = "TenantAdmin",
                CompanyId = company.Id
            };

            _context.Users.Add(tenantAdmin);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Company Created Successfully",

                company = new
                {
                    company.Id,
                    company.Name,
                    company.CompanyName,
                    company.Email
                },

                tenantAdmin = new
                {
                    tenantAdmin.Id,
                    tenantAdmin.Name,
                    tenantAdmin.Email,
                    tenantAdmin.Role
                }
            });
        }
        [HttpPut("{id}")]
            public async Task<IActionResult> Putcompany(int id, [FromBody] CompanyN Update)
        {
            var company = await _context.companyName.FindAsync(id);

            if (company == null)
            {
                return NotFound();
            }
            if (CurrentRole() != "SuperAdmin")
            {
                if (company.Id != CurrentCompany())
                {
                    return Unauthorized();
                }
            }
            if (id != Update.Id)
            {
                return BadRequest(new { message = "this id data is not avaible" });
            }

            _context.Entry(Update).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exist = await _context.companyName.AnyAsync(e => e.Id == id);
                if (!exist)
                {
                    return NotFound(new { message = "This data is not found" });
                }
                else
                {
                    throw;
                }
            }
            return Ok(new { message = "Company data are succesfully Update", data = Update });
        }




        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]   
        public async Task<IActionResult> deletecompany(int id)
        {
            var names = await _context.companyName.FindAsync(id);
            if (names == null)
            {
                return NotFound(new { message = "This Id data is not avaible" });
            }
            _context.companyName.Remove(names);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Delete successfully" });
        }
    }
}
