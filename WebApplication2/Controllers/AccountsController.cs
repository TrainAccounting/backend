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
                AccountValue = a.AccountValue,
                DateOfOpening = a.DateOfOpening
            }).ToListAsync();

        [HttpGet("by-record/{recordId}")]
        public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccountsByRecord(int recordId) =>
            await _context.Accounts.Where(a => a.RecordId == recordId).Select(a => new AccountDto
            {
                Id = a.Id,
                NameOfAccount = a.NameOfAccount,
                AccountValue = a.AccountValue,
                DateOfOpening = a.DateOfOpening
            }).ToListAsync();

        [HttpPost]
        public async Task<ActionResult<AccountDto>> CreateAccount(AccountCreateDto dto)
        {
            var record = await _context.Records.FindAsync(dto.RecordId);
            if (record == null) return NotFound("Record not found");

            var account = new Account
            {
                NameOfAccount = dto.NameOfAccount,
                AccountValue = dto.AccountValue,
                DateOfOpening = DateTime.UtcNow,
                RecordId = dto.RecordId
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAccounts), new AccountDto
            {
                Id = account.Id,
                NameOfAccount = account.NameOfAccount,
                AccountValue = account.AccountValue,
                DateOfOpening = account.DateOfOpening
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccount(int id, AccountUpdateDto dto)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return NotFound();

            account.NameOfAccount = dto.NameOfAccount ?? account.NameOfAccount;
            account.AccountValue = dto.AccountValue ?? account.AccountValue;

            await _context.SaveChangesAsync();
            return NoContent();
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
    }
}