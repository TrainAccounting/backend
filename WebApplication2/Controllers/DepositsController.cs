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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepositDto>>> GetDeposits()
        {
            return await _context.Deposits.Select(d => new DepositDto
            {
                Id = d.Id,
                NameOfDeposit = d.NameOfDeposit,
                DepositStartValue = d.DepositStartValue,
                DepositCurrentValue = d.DepositCurrentValue,
                DateOfOpening = d.DateOfOpening,
                PeriodOfPayment = d.PeriodOfPayment,
                InterestRate = d.InterestRate,
                Capitalisation = d.Capitalisation,
                Amount = d.Amount,
                PayType = d.PayType,
                IsActive = d.IsActive
            }).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DepositDto>> GetDeposit(int id)
        {
            var deposit = await _context.Deposits.FindAsync(id);
            if (deposit == null) return NotFound();
            return new DepositDto
            {
                Id = deposit.Id,
                NameOfDeposit = deposit.NameOfDeposit,
                DepositStartValue = deposit.DepositStartValue,
                DepositCurrentValue = deposit.DepositCurrentValue,
                DateOfOpening = deposit.DateOfOpening,
                PeriodOfPayment = deposit.PeriodOfPayment,
                InterestRate = deposit.InterestRate,
                Capitalisation = deposit.Capitalisation,
                Amount = deposit.Amount,
                PayType = deposit.PayType,
                IsActive = deposit.IsActive
            };
        }

        [HttpPost]
        public async Task<ActionResult<DepositDto>> CreateDeposit(DepositDto dto)
        {
            var deposit = new Deposit
            {
                NameOfDeposit = dto.NameOfDeposit,
                DepositStartValue = dto.DepositStartValue,
                DepositCurrentValue = dto.DepositCurrentValue,
                DateOfOpening = dto.DateOfOpening,
                PeriodOfPayment = dto.PeriodOfPayment,
                InterestRate = dto.InterestRate,
                Capitalisation = dto.Capitalisation,
                Amount = dto.Amount,
                PayType = dto.PayType,
                IsActive = dto.IsActive,
                RecordId = dto.Id // предполагается, что RecordId приходит в dto.Id, иначе скорректировать
            };
            _context.Deposits.Add(deposit);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetDeposit), new { id = deposit.Id }, dto);
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