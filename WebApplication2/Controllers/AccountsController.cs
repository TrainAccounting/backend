using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trainacc.Data;
using Trainacc.Models;

namespace Trainacc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AccountsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccounts()
        {
            return await _context.Accounts
                .Select(a => new AccountDto
                {
                    Id = a.Id,
                    NameOfAccount = a.NameOfAccount,
                    AccountValue = a.AccountValue,
                    DateOfOpening = a.DateOfOpening
                })
                .ToListAsync();
        }

        [HttpGet("by-record/{recordId}")]
        public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccountsByRecord(int recordId)
        {
            return await _context.Accounts
                .Where(a => a.RecordId == recordId)
                .Select(a => new AccountDto
                {
                    Id = a.Id,
                    NameOfAccount = a.NameOfAccount,
                    AccountValue = a.AccountValue,
                    DateOfOpening = a.DateOfOpening
                })
                .ToListAsync();
        }
    }
}