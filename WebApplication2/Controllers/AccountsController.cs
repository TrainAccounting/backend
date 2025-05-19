using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trainacc.Filters;
using Trainacc.Models;
using Trainacc.Services;

namespace Trainacc.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(LogActionFilter))]
    [ServiceFilter(typeof(ETagFilter))]
    public class AccountsController : ControllerBase
    {
        private readonly AccountsService _service;
        public AccountsController(AccountsService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccounts()
        {
            try { return await _service.GetAccountsAsync(); }
            catch { return Problem(); }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AccountDto>> GetAccount(int id)
        {
            try
            {
                var result = await _service.GetAccountAsync(id);
                if (result == null) return NotFound();
                return result;
            }
            catch { return Problem(); }
        }

        [HttpPost]
        public async Task<ActionResult<AccountDto>> CreateAccount(AccountCreateDto dto)
        {
            try
            {
                var created = await _service.CreateAccountAsync(dto);
                if (created == null) return NotFound("Record not found");
                return CreatedAtAction(nameof(GetAccount), new { id = created.Id }, created);
            }
            catch { return Problem(); }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccount(int id, AccountUpdateDto dto)
        {
            try
            {
                var ok = await _service.UpdateAccountAsync(id, dto);
                if (!ok) return NotFound();
                return NoContent();
            }
            catch { return Problem(); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            try
            {
                var ok = await _service.DeleteAccountAsync(id);
                if (!ok) return NotFound();
                return NoContent();
            }
            catch { return Problem(); }
        }

        [HttpGet("user-balance")]
        public async Task<ActionResult<decimal>> GetUserTotalBalance()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var balance = await _service.GetUserTotalBalanceAsync(userId);
                return Ok(balance);
            }
            catch { return Problem(); }
        }

        [HttpGet("user-accounts-summary")]
        public async Task<ActionResult<List<AccountSummaryDto>>> GetAccountSummaries()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _service.GetAccountSummariesAsync(userId);
                return Ok(result);
            }
            catch { return Problem(); }
        }

        [HttpGet("by-record/{recordId}")]
        public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccountsByRecord(int recordId)
        {
            try { return await _service.GetAccountsByRecordAsync(recordId); }
            catch { return Problem(); }
        }

        [HttpGet("balance-history")]
        public async Task<ActionResult<List<object>>> GetBalanceHistory([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var history = await _service.GetBalanceHistoryAsync(userId, from, to);
                return Ok(history.Select(x => new { date = x.Date, balance = x.Balance }));
            }
            catch { return Problem(); }
        }
    }
}