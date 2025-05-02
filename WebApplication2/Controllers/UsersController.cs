using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trainacc.Data;
using Trainacc.Models;

namespace Trainacc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Users>>> GetUsers()
        {
            return await _context.Users.Include(u => u.Records).ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Users>> CreateUser(Users user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user);
        }
    }
}