using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trainacc.Data;
using Trainacc.Models;
using Trainacc.Filters;

namespace Trainacc.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(LogActionFilter))]
    [ServiceFilter(typeof(ETagFilter))]
    public class AccountsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AccountsController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccounts() =>
            await _context.Accounts.Select(a => new AccountDto
            {
                Id = a.Id,
                NameOfAccount = a.NameOfAccount,
                DateOfOpening = a.DateOfOpening,
                Balance = a.Balance
            }).ToListAsync();

        [HttpGet("by-record/{recordId}")]
        public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccountsByRecord(int recordId) =>
            await _context.Accounts.Where(a => a.RecordId == recordId).Select(a => new AccountDto
            {
                Id = a.Id,
                NameOfAccount = a.NameOfAccount,
                DateOfOpening = a.DateOfOpening,
                Balance = a.Balance
            }).ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<AccountDto>> GetAccount(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return NotFound();
            return new AccountDto
            {
                Id = account.Id,
                NameOfAccount = account.NameOfAccount ?? string.Empty,
                DateOfOpening = account.DateOfOpening,
                Balance = account.Balance
            };
        }

        [HttpPost]
        public async Task<ActionResult<AccountDto>> CreateAccount(AccountCreateDto dto)
        {
            var recordId = GetRecordId();
            var record = await _context.Records.FindAsync(dto.RecordId);
            if (record == null) return NotFound("Record not found");

            var account = new Account
            {
                NameOfAccount = dto.NameOfAccount,
                DateOfOpening = DateTime.UtcNow,
                RecordId = dto.RecordId,
                Balance = 0
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAccounts), new AccountDto
            {
                Id = account.Id,
                NameOfAccount = account.NameOfAccount,
                DateOfOpening = account.DateOfOpening,
                Balance = account.Balance
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccount(int id, AccountUpdateDto dto)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return NotFound();

            account.NameOfAccount = dto.NameOfAccount ?? account.NameOfAccount;

            await _context.SaveChangesAsync();
            return Ok(new AccountDto
            {
                Id = account.Id,
                NameOfAccount = account.NameOfAccount ?? string.Empty,
                DateOfOpening = account.DateOfOpening,
                Balance = account.Balance
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return NotFound();

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("balance")]
        public async Task<ActionResult<decimal>> GetTotalBalance()
        {
            var recordId = GetRecordId();
            var record = await _context.Records.Include(r => r.Accounts).FirstOrDefaultAsync(r => r.Id == recordId);
            if (record == null)
            {
                return NotFound("Record not found for the current user.");
            }
            return record.Accounts.Sum(a => a.Balance);
        }

        [HttpGet("report/{recordId}")]
        public async Task<ActionResult<IEnumerable<AccountReportDto>>> GetAccountReport(int recordId)
        {
            var record = await _context.Records.Include(r => r.Accounts).ThenInclude(r => r.Transactions).FirstOrDefaultAsync(r => r.Id == recordId);
            if (record == null) return NotFound("Record not found");

            var report = record.Accounts.Select(a => new AccountReportDto
            {
                AccountName = a.NameOfAccount ?? string.Empty,
                TotalTransactions = a.Transactions.Count,
                TotalValue = a.Transactions.Sum(t => t.TransactionValue)
            }).ToList();

            return Ok(report);
        }

        private int GetRecordId()
        {
            var recordIdClaim = User.FindFirst("RecordId");
            if (recordIdClaim == null)
            {
                throw new NullReferenceException("RecordId claim is missing.");
            }
            return int.Parse(recordIdClaim.Value);
        }
    }
}