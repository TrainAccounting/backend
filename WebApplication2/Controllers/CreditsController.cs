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
    public class CreditsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CreditsController(AppDbContext context) => _context = context;

        [HttpGet("by-record/{recordId}")]
        public async Task<ActionResult<IEnumerable<CreditDto>>> GetCreditsByRecord(int recordId) =>
            await _context.Credits
                .Where(c => c.RecordId == recordId)
                .Select(c => new CreditDto
                {
                    Id = c.Id,
                    NameOfCredit = c.NameOfCredit,
                    CreditCurrentValue = c.CreditCurrentValue,
                    DateOfOpening = c.DateOfOpening,
                    PeriodOfPayment = c.PeriodOfPayment,
                    InterestRate = c.InterestRate,
                    Amount = c.Amount,
                    PayType = c.PayType
                })
                .ToListAsync();

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<CreditDto>>> GetAllCredits()
        {
            var recordId = GetRecordId();
            return await _context.Credits
                .Where(c => c.RecordId == recordId)
                .Select(c => new CreditDto
                {
                    Id = c.Id,
                    NameOfCredit = c.NameOfCredit,
                    CreditCurrentValue = c.CreditCurrentValue,
                    DateOfOpening = c.DateOfOpening,
                    PeriodOfPayment = c.PeriodOfPayment,
                    InterestRate = c.InterestRate,
                    PayType = c.PayType
                })
                .ToListAsync();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCredit([FromBody] CreditDto dto)
        {
            var recordId = GetRecordId();
            var credit = new Credit
            {
                NameOfCredit = dto.NameOfCredit,
                CreditCurrentValue = dto.CreditCurrentValue,
                DateOfOpening = dto.DateOfOpening,
                PeriodOfPayment = dto.PeriodOfPayment,
                InterestRate = dto.InterestRate,
                Amount = dto.Amount,
                PayType = dto.PayType,
                IsActive = dto.IsActive,
                RecordId = recordId
            };

            _context.Credits.Add(credit);
            await _context.SaveChangesAsync();

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.RecordId == recordId);
            if (account != null && credit.Amount > 0)
            {
                var transaction = new Transactions
                {
                    Category = "Credit",
                    TransactionValue = -Math.Abs(credit.Amount),
                    TimeOfTransaction = DateTime.UtcNow,
                    RecordId = recordId
                };
                _context.Transactions.Add(transaction);
                account.Balance -= Math.Abs(credit.Amount);
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetAllCredits), new { id = credit.Id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCredit(int id, CreditUpdateDto dto)
        {
            var credit = await _context.Credits.FindAsync(id);
            if (credit == null) return NotFound();

            credit.NameOfCredit = dto.NameOfCredit ?? credit.NameOfCredit;
            credit.CreditCurrentValue = dto.CreditCurrentValue ?? credit.CreditCurrentValue;
            credit.PeriodOfPayment = dto.PeriodOfPayment ?? credit.PeriodOfPayment;
            credit.InterestRate = dto.InterestRate ?? credit.InterestRate;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCredit(int id)
        {
            var credit = await _context.Credits.FindAsync(id);
            if (credit == null) return NotFound();

            _context.Credits.Remove(credit);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("summary")]
        public async Task<ActionResult<CreditSummaryDto>> GetCreditSummary()
        {
            var recordId = GetRecordId();
            var credits = await _context.Credits.Where(c => c.RecordId == recordId).ToListAsync();

            return new CreditSummaryDto
            {
                TotalCredits = (int)credits.Sum(d => d.CreditCurrentValue),
                ActiveCredits = credits.Count(c => c.IsActive)
            };
        }

        [HttpGet("report/{recordId}")]
        public async Task<ActionResult<IEnumerable<CreditReportDto>>> GetCreditReport(int recordId)
        {
            var record = await _context.Records.Include(r => r.Credits).FirstOrDefaultAsync(r => r.Id == recordId);
            if (record == null) return NotFound("Record not found");

            var report = record.Credits.Select(c => new CreditReportDto
            {
                CreditId = c.Id,
                Name = c.NameOfCredit,
                CurrentValue = c.CreditCurrentValue,
                InterestRate = c.InterestRate,
                PeriodOfPayment = c.PeriodOfPayment
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