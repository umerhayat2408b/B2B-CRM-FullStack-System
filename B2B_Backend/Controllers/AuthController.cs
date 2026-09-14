using B2B_PRO.Models;
using B2B_PRO.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace B2B_PRO.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly AuditService _auditService;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(
      AppDbContext context,
      IConfiguration configuration,
      AuditService auditService)
        {
            _context = context;
            _configuration = configuration;
            _auditService = auditService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {

            var user = await _context.Users
    .FirstOrDefaultAsync(x => x.Email == model.Email);

            if (user == null)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid Email or Password"
                });
            }
            if (model.Password != user.Password)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid Email or Password"
                });
            }
            await _auditService.SaveLog(
    "LOGIN",
    user.Name,
    user.Role,
    user.CompanyId.Value,
    "User Login Successful");
           

           

            var claims = new[]
 {
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Name, user.Name),
    new Claim(ClaimTypes.Email, user.Email),
    new Claim(ClaimTypes.Role, user.Role),
        new Claim("CompanyId", user.CompanyId.ToString())
};

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(
                    Convert.ToDouble(_configuration["Jwt:DurationInMinutes"])
                ),
                signingCredentials: creds
            );

            return Ok(new
            {
                success = true,
                token = new JwtSecurityTokenHandler().WriteToken(token),
                user = new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Role
                }
            });
        }
    
   
    }

}


