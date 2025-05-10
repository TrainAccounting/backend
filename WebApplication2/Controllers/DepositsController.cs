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
    public class DepositsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DepositsController(AppDbContext context) => _context = context;

        [HttpGet("by-record/{recordId}")]
        public async Task<ActionResult<IEnumerable<DepositDto>>> GetDepositsByRecord(int recordId) =>
            await _context.Deposits
                .Where(d => d.RecordId == recordId)
                .Select(d => new DepositDto
                {
                    Id = d.Id,
                    NameOfDeposit = d.NameOfDeposit,
                    DepositStartValue = d.DepositStartValue,
                    DepositCurrentValue = d.DepositCurrentValue,
                    DateOfOpening = d.DateOfOpening,
                    PeriodOfPayment = d.PeriodOfPayment,
                    InterestRate = d.InterestRate,
                    Capitalisation = d.Capitalisation,
                    PayType = d.PayType
                })
                .ToListAsync();

        [HttpPost]
        public async Task<ActionResult<DepositDto>> CreateDeposit(DepositCreateDto dto)
        {
            var record = await _context.Records.FindAsync(dto.RecordId);
            if (record == null) return NotFound("Record not found");

            var deposit = new Deposit
            {
                NameOfDeposit = dto.NameOfDeposit,
                DepositStartValue = dto.DepositStartValue,
                DepositCurrentValue = dto.DepositStartValue,
                DateOfOpening = DateTime.UtcNow,
                PeriodOfPayment = dto.PeriodOfPayment,
                InterestRate = dto.InterestRate,
                Capitalisation = dto.Capitalisation,
                PayType = dto.PayType,
                RecordId = dto.RecordId
            };

            _context.Deposits.Add(deposit);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDepositsByRecord), new { recordId = dto.RecordId }, new DepositDto
            {
                Id = deposit.Id,
                NameOfDeposit = deposit.NameOfDeposit,
                DepositStartValue = deposit.DepositStartValue,
                DepositCurrentValue = deposit.DepositCurrentValue,
                DateOfOpening = deposit.DateOfOpening,
                PeriodOfPayment = deposit.PeriodOfPayment,
                InterestRate = deposit.InterestRate,
                Capitalisation = deposit.Capitalisation,
                PayType = deposit.PayType
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDeposit(int id, DepositUpdateDto dto)
        {
            var deposit = await _context.Deposits.FindAsync(id);
            if (deposit == null) return NotFound();

            deposit.NameOfDeposit = dto.NameOfDeposit ?? deposit.NameOfDeposit;
            deposit.DepositCurrentValue = dto.DepositCurrentValue ?? deposit.DepositCurrentValue;
            deposit.PeriodOfPayment = dto.PeriodOfPayment ?? deposit.PeriodOfPayment;
            deposit.InterestRate = dto.InterestRate ?? deposit.InterestRate;
            deposit.Capitalisation = dto.Capitalisation ?? deposit.Capitalisation;
            deposit.PayType = dto.PayType ?? deposit.PayType;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeposit(int id)
        {
            var deposit = await _context.Deposits.FindAsync(id);
            if (deposit == null) return NotFound();

            _context.Deposits.Remove(deposit);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}