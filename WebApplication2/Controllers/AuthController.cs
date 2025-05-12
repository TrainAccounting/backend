using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trainacc.Data;
using Trainacc.Models;
using Trainacc.Services;
using Trainacc.Filters;

namespace Trainacc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(LogActionFilter))]
    [ServiceFilter(typeof(ETagFilter))]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;

        public AuthController(AppDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserAuthDto>> Register(UserCreateDto userDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
                return BadRequest("Email already exists.");

            var user = new Users
            {
                FIO = userDto.FIO,
                Email = userDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
                Role = userDto.Role ?? string.Empty
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var record = new Record
            {
                NameOfRecord = "Default Record",
                DateOfCreation = DateTime.UtcNow,
                UserId = user.Id
            };
            _context.Records.Add(record);
            await _context.SaveChangesAsync();

            record.UserId = user.Id;
            _context.Records.Update(record);
            await _context.SaveChangesAsync();

            var account = new Account
            {
                NameOfAccount = "Default Account",
                Balance = 0,
                DateOfOpening = DateTime.UtcNow,
                RecordId = record.UserId
            };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            var token = _tokenService.GenerateToken(user, record.Id);

            return new UserAuthDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Role = user.Role ?? string.Empty,
                Token = token
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserAuthDto>> Login(UserLoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password.");

            var record = await _context.Records.FirstOrDefaultAsync(r => r.UserId == user.Id);
            int? recordId = record?.Id;
            var token = _tokenService.GenerateToken(user, recordId);

            return new UserAuthDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Role = user.Role ?? string.Empty,
                Token = token
            };
        }
    }
}