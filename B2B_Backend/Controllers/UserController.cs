using B2B_PRO.Models;
using B2B_PRO.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace B2B_PRO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuditService _auditService;

        public UserController(AppDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        private int CurrentCompany() => Convert.ToInt32(User.FindFirst("CompanyId")?.Value);
        private int CurrentUserId() => Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        private string CurrentRole() => User.FindFirst(ClaimTypes.Role)?.Value ?? "";


        [HttpGet]
        public async Task<IActionResult> Username()
        {
            if (CurrentRole() == "SuperAdmin")
                return Ok(await _context.Users.ToListAsync());

            if (CurrentRole() == "TenantAdmin")
                return Ok(await _context.Users.Where(x => x.CompanyId == CurrentCompany()).ToListAsync());

            return Ok(await _context.Users.Where(x => x.Id == CurrentUserId()).ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> userbyid(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "This user is not found" });

            if (CurrentRole() == "TenantAdmin" && user.CompanyId != CurrentCompany())
                return Unauthorized();

            if (CurrentRole() == "Employee" && user.Id != CurrentUserId())
                return Unauthorized();

            return Ok(user);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,TenantAdmin")]
        public async Task<IActionResult> CreateUser([FromBody] User userpost)
        {
            if (userpost == null)
            {
                return BadRequest(new { message = "Please enter valid data" });
            }

            string currentRole = CurrentRole();

            // TenantAdmin apni company ke andar hi Employee bana sakta hai
            if (currentRole == "TenantAdmin")
            {
                userpost.CompanyId = CurrentCompany();

                if (userpost.Role == "SuperAdmin")
                {
                    return BadRequest(new
                    {
                        message = "Tenant Admin cannot create SuperAdmin."
                    });
                }

                if (userpost.Role == "TenantAdmin")
                {
                    return Unauthorized(new
                    {
                        message = "Tenant Admin cannot create another Tenant Admin."
                    });
                }

                userpost.Role = "Employee";
            }

            // CompanyId check
            if (userpost.CompanyId == null)
            {
                return BadRequest(new
                {
                    message = "CompanyId is required."
                });
            }

            // Existing company database se find karo
            var company = await _context.companyName
                .FirstOrDefaultAsync(x => x.Id == userpost.CompanyId);

            if (company == null)
            {
                return BadRequest(new
                {
                    message = "Company not found."
                });
            }

            // IMPORTANT:
            // Existing company ko navigation property mein attach karo
            userpost.Company = company;

            // New User ka ID database khud generate karega
            userpost.Id = 0;

            _context.Users.Add(userpost);

            await _context.SaveChangesAsync();

            await _auditService.SaveLog(
                "CREATE USER",
                userpost.Name,
                userpost.Role,
                userpost.CompanyId ?? 0,
                "New User Created"
            );

            return Ok(new
            {
                message = "User Created Successfully",
                data = new
                {
                    userpost.Id,
                    userpost.Name,
                    userpost.Email,
                    userpost.Role,
                    userpost.CompanyId
                }
            });
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,TenantAdmin")]
        public async Task<IActionResult> Edituser(int id, [FromBody] User useredit)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null || id != useredit.Id)
                return NotFound(new { message = "User not found" });

            if (CurrentRole() == "TenantAdmin" && existingUser.CompanyId != CurrentCompany())
                return Unauthorized();

            if (CurrentRole() == "Employee" && existingUser.Id != CurrentUserId())
                return Unauthorized();

            _context.Entry(useredit).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Users.AnyAsync(e => e.Id == id))
                    return NotFound(new { message = "This id is not found" });
                else
                    throw;
            }

            return Ok(new { message = "User data is Successfully Updated" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,TenantAdmin")]
        public async Task<IActionResult> deleteuser(int id)
        {
            var userp = await _context.Users.FindAsync(id);

            if (userp == null)
                return NotFound(new { message = "This user is not found" });

            if (CurrentRole() == "TenantAdmin" && userp.CompanyId != CurrentCompany())
                return Unauthorized();

            _context.Users.Remove(userp);
            await _context.SaveChangesAsync();

            await _auditService.SaveLog(
                "DELETE USER",
                userp.Name,
                userp.Role,
                userp.CompanyId ?? 0,
                "User Deleted"
            );

            return Ok(new { message = "User deleted successfully" });
        }
    }
}