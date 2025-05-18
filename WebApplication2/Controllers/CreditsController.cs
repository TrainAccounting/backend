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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CreditDto>>> GetCredits()
        {
            return await _context.Credits.Select(c => new CreditDto
            {
                Id = c.Id,
                NameOfCredit = c.NameOfCredit,
                CreditCurrentValue = c.CreditCurrentValue,
                DateOfOpening = c.DateOfOpening,
                PeriodOfPayment = c.PeriodOfPayment,
                InterestRate = c.InterestRate,
                Amount = c.Amount,
                PayType = c.PayType,
                IsActive = c.IsActive
            }).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CreditDto>> GetCredit(int id)
        {
            var credit = await _context.Credits.FindAsync(id);
            if (credit == null) return NotFound();
            return new CreditDto
            {
                Id = credit.Id,
                NameOfCredit = credit.NameOfCredit,
                CreditCurrentValue = credit.CreditCurrentValue,
                DateOfOpening = credit.DateOfOpening,
                PeriodOfPayment = credit.PeriodOfPayment,
                InterestRate = credit.InterestRate,
                Amount = credit.Amount,
                PayType = credit.PayType,
                IsActive = credit.IsActive
            };
        }

        [HttpPost]
        public async Task<ActionResult<CreditDto>> CreateCredit(CreditDto dto)
        {
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
                RecordId = dto.Id // предполагается, что RecordId приходит в dto.Id, иначе скорректировать
            };
            _context.Credits.Add(credit);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCredit), new { id = credit.Id }, dto);
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
    }
}