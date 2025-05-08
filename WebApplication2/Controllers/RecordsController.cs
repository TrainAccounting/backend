using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trainacc.Data;
using Trainacc.Filters;
using Trainacc.Models;

namespace Trainacc.Controllers
{
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
            return await _context.Records
                .Include(r => r.User)
                .Select(r => new RecordDto
                {
                    Id = r.Id,
                    NameOfRecord = r.NameOfRecord,
                    DateOfCreation = r.DateOfCreation,
                    User = new UserDto
                    {
                        Id = r.User.Id,
                        FIO = r.User.FIO,
                        Email = r.User.Email,
                        Phone = r.User.Phone
                    }
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RecordDto>> GetRecord(int id)
        {
            var record = await _context.Records
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (record == null)
            {
                return NotFound();
            }

            return new RecordDto
            {
                Id = record.Id,
                NameOfRecord = record.NameOfRecord,
                DateOfCreation = record.DateOfCreation,
                User = new UserDto
                {
                    Id = record.User.Id,
                    FIO = record.User.FIO,
                    Email = record.User.Email,
                    Phone = record.User.Phone
                }
            };
        }

        [HttpGet("{id}/full")]
        public async Task<ActionResult<RecordDto>> GetRecordDetails(int id)
        {
            var record = await _context.Records
                .Include(r => r.User)
                .Include(r => r.Accounts)
                .Include(r => r.Transactions)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (record == null)
            {
                return NotFound();
            }

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
                } : null,
                Accounts = record.Accounts.Select(a => new AccountSummaryDto
                {
                    Id = a.Id,
                    NameOfAccount = a.NameOfAccount,
                    AccountValue = a.AccountValue
                }).ToList(),
                Transactions = record.Transactions.Select(t => new TransactionSummaryDto
                {
                    Id = t.Id,
                    Category = t.Category,
                    TransactionValue = t.TransactionValue
                }).ToList()
            };
        }

        [HttpPost]
        public async Task<ActionResult<RecordDto>> CreateRecord(RecordCreateDto recordDto)
        {
            var user = await _context.Users.FindAsync(recordDto.UserId);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var record = new Record
            {
                NameOfRecord = recordDto.NameOfRecord,
                DateOfCreation = DateTime.UtcNow,
                UserId = recordDto.UserId
            };

            _context.Records.Add(record);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRecord), new { id = record.Id }, new RecordDto
            {
                Id = record.Id,
                NameOfRecord = record.NameOfRecord,
                DateOfCreation = record.DateOfCreation,
                User = new UserDto
                {
                    Id = user.Id,
                    FIO = user.FIO,
                    Email = user.Email,
                    Phone = user.Phone
                }
            });
        }
    }
}