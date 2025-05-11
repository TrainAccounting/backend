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

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<DepositDto>>> GetAllDeposits()
        {
            var recordId = GetRecordId();
            return await _context.Deposits
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
        }

        [HttpPost]
        public async Task<IActionResult> CreateDeposit([FromBody] DepositDto dto)
        {
            var recordId = GetRecordId();

            var deposit = new Deposit
            {
                NameOfDeposit = dto.NameOfDeposit,
                DepositStartValue = dto.DepositStartValue,
                DepositCurrentValue = dto.DepositCurrentValue,
                DateOfOpening = dto.DateOfOpening,
                PeriodOfPayment = dto.PeriodOfPayment,
                InterestRate = dto.InterestRate,
                Capitalisation = dto.Capitalisation,
                PayType = dto.PayType,
                RecordId = recordId
            };

            _context.Deposits.Add(deposit);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllDeposits), new { id = deposit.Id }, dto);
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

       [HttpGet("summary")]
        public async Task<ActionResult<DepositSummaryDto>> GetDepositSummary()
        {
            var recordId = GetRecordId();
            var deposits = await _context.Deposits.Where(d => d.RecordId == recordId).ToListAsync();

            return new DepositSummaryDto
            {
                TotalDeposits = (int)deposits.Sum(d => d.DepositCurrentValue),
                ActiveDeposits = deposits.Count(d => d.IsActive)
            };
        }

        [HttpGet("report/{recordId}")]
        public async Task<ActionResult<IEnumerable<DepositReportDto>>> GetDepositReport(int recordId)
        {
            var record = await _context.Records.Include(r => r.Deposits).FirstOrDefaultAsync(r => r.Id == recordId);
            if (record == null) return NotFound("Record not found");

            var report = record.Deposits.Select(d => new DepositReportDto
            {
                DepositId = d.Id,
                Name = d.NameOfDeposit,
                CurrentValue = d.DepositCurrentValue,
                InterestRate = d.InterestRate
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