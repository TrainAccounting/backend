using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trainacc.Data;
using Trainacc.Models;
using Trainacc.Filters;

namespace Trainacc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(LogActionFilter))]
    [ServiceFilter(typeof(ETagFilter))]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(AppDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            return await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FIO = u.FIO,
                    Email = u.Email,
                    Phone = u.Phone

                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return new UserDto
            {
                Id = user.Id,
                FIO = user.FIO,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role
            };
        }

        [HttpGet("{id}/full")]
        public async Task<ActionResult<UserWithRecordsDto>> GetUserWithRecords(int id)
        {
            var user = await _context.Users
                .Include(u => u.Records)
                .ThenInclude(r => r.Accounts)
                .Include(u => u.Records)
                .ThenInclude(r => r.Transactions)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return new UserWithRecordsDto
            {
                Id = user.Id,
                FIO = user.FIO,
                Email = user.Email,
                Phone = user.Phone,
                Records = user.Records.Select(r => new RecordSummaryDto
                {
                    Id = r.Id,
                    NameOfRecord = r.NameOfRecord,
                    DateOfCreation = r.DateOfCreation
                }).ToList()
            };
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(UserCreateDto userDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
                return BadRequest("Email already exists");

            var user = new Users
            {
                FIO = userDto.FIO,
                Email = userDto.Email,
                Phone = userDto.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
                Role = userDto.Role ?? "User" 
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new UserDto
            {
                Id = user.Id,
                FIO = user.FIO,
                Email = user.Email,
                Phone = user.Phone,
                PasswordHash = user.PasswordHash,
                Role = user.Role
            });
        }

        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<IActionResult> UpdateUser(int id, UserUpdateDto userDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(userDto.FIO))
                user.FIO = userDto.FIO;

            if (!string.IsNullOrWhiteSpace(userDto.Email))
                user.Email = userDto.Email;

            if (!string.IsNullOrWhiteSpace(userDto.Phone))
                user.Phone = userDto.Phone;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}/records")]
        public async Task<ActionResult<IEnumerable<RecordDto>>> GetUserRecords(int id)
        {
            var user = await _context.Users
                .Include(u => u.Records)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return user.Records.Select(r => new RecordDto
            {
                Id = r.Id,
                NameOfRecord = r.NameOfRecord,
                DateOfCreation = r.DateOfCreation
            }).ToList();
        }
    }
}