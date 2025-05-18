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
        public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccounts()
        {
            return await _context.Accounts.Select(a => new AccountDto
            {
                Id = a.Id,
                NameOfAccount = a.NameOfAccount,
                DateOfOpening = a.DateOfOpening,
                Balance = a.Balance
            }).ToListAsync();
        }

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
            return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, new AccountDto
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