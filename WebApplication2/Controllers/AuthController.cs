using Microsoft.AspNetCore.Mvc;
using Trainacc.Models;
using Trainacc.Services;
using Microsoft.AspNetCore.Authorization;
using Trainacc.Data;

namespace Trainacc.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly AppDbContext _context;

        public AuthController(TokenService tokenService, AppDbContext context)
        {
            _tokenService = tokenService;
            _context = context;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials");

            var token = _tokenService.GenerateToken(user.Email, "User");
            return Ok(new { Token = token });
        }

        private bool VerifyPassword(string password, string storedHash)
            => BCrypt.Net.BCrypt.Verify(password, storedHash);
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}