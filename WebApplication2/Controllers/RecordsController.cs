using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Trainacc.Data;
using Trainacc.Filters;
using Trainacc.Models;

namespace Trainacc.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(LogActionFilter))]
    [ServiceFilter(typeof(ETagFilter))]
    public class RecordsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RecordsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecordDto>>> GetRecords()
        {
            return await _context.Records.Include(r => r.User).Select(r => new RecordDto
            {
                Id = r.Id,
                NameOfRecord = r.NameOfRecord,
                DateOfCreation = r.DateOfCreation,
                User = r.User != null ? new UserDto
                {
                    Id = r.User.Id,
                    FIO = r.User.FIO,
                    Email = r.User.Email,
                    Phone = r.User.Phone
                } : null
            }).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RecordDto>> GetRecord(int id)
        {
            var record = await _context.Records.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);
            if (record == null) return NotFound();
            return new RecordDto
            {
                Id = record.Id,
                NameOfRecord = record.NameOfRecord,
                DateOfCreation = record.DateOfCreation,
                User = record.User != null ? new UserDto
                {
                    Id = record.User.Id,
                    FIO = record.User.FIO,
                    Email = record.User.Email,
                    Phone = record.User.Phone
                } : null
            };
        }

        [HttpPost]
        public async Task<ActionResult<RecordDto>> CreateRecord(RecordCreateDto recordDto)
        {
            // Здесь нужно получить userId из контекста или параметра, например из JWT
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var record = new Record
            {
                NameOfRecord = recordDto.NameOfRecord,
                DateOfCreation = DateTime.UtcNow,
                UserId = userId
            };
            _context.Records.Add(record);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRecord), new { id = record.Id }, new RecordDto
            {
                Id = record.Id,
                NameOfRecord = record.NameOfRecord,
                DateOfCreation = record.DateOfCreation
            });
        }
    }
}