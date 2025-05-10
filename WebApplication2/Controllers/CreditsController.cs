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
                    PayType = c.PayType
                })
                .ToListAsync();

        [HttpPost]
        public async Task<ActionResult<CreditDto>> CreateCredit(CreditCreateDto dto)
        {
            var record = await _context.Records.FindAsync(dto.RecordId);
            if (record == null) return NotFound("Record not found");

            var credit = new Credit
            {
                NameOfCredit = dto.NameOfCredit,
                CreditCurrentValue = dto.CreditCurrentValue,
                DateOfOpening = DateTime.UtcNow,
                PeriodOfPayment = dto.PeriodOfPayment,
                InterestRate = dto.InterestRate,
                PayType = dto.PayType,
                RecordId = dto.RecordId
            };

            _context.Credits.Add(credit);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCreditsByRecord), new { recordId = dto.RecordId }, new CreditDto
            {
                Id = credit.Id,
                NameOfCredit = credit.NameOfCredit,
                CreditCurrentValue = credit.CreditCurrentValue,
                DateOfOpening = credit.DateOfOpening,
                PeriodOfPayment = credit.PeriodOfPayment,
                InterestRate = credit.InterestRate,
                PayType = credit.PayType
            });
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
            credit.PayType = dto.PayType ?? credit.PayType;

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
    }
}