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
    public class RestrictionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RestrictionsController(AppDbContext context) => _context = context;

        [HttpGet("by-record/{recordId}")]
        public async Task<ActionResult<IEnumerable<RestrictionDto>>> GetRestrictionsByRecord(int recordId) =>
            await _context.Restrictions
                .Where(r => r.RecordId == recordId)
                .Select(r => new RestrictionDto
                {
                    Id = r.Id,
                    Category = r.Category,
                    RestrictionValue = r.RestrictionValue,
                    MoneySpent = r.MoneySpent,
                    Name = r.Name,
                    Description = r.Description,
                    IsActive = r.IsActive
                })
                .ToListAsync();

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<RestrictionDto>>> GetAllRestrictions()
        {
            var recordId = GetRecordId();
            return await _context.Restrictions
                .Where(r => r.RecordId == recordId)
                .Select(r => new RestrictionDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    IsActive = r.IsActive
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RestrictionDto>> GetRestriction(int id)
        {
            var restriction = await _context.Restrictions.FindAsync(id);
            if (restriction == null) return NotFound();
            return new RestrictionDto
            {
                Id = restriction.Id,
                Category = restriction.Category ?? string.Empty,
                RestrictionValue = restriction.RestrictionValue,
                MoneySpent = restriction.MoneySpent,
                Name = restriction.Name ?? string.Empty,
                Description = restriction.Description ?? string.Empty,
                IsActive = restriction.IsActive
            };
        }

        [HttpPost]
        public async Task<IActionResult> CreateRestriction([FromBody] RestrictionDto dto)
        {
            var recordId = GetRecordId();
            var sum = await _context.Transactions
                .Where(t => t.RecordId == recordId && t.Category == dto.Category)
                .SumAsync(t => t.TransactionValue);
            var moneySpent = Math.Abs(sum);

            var restriction = new Restriction
            {
                Name = dto.Name,
                Description = dto.Description,
                IsActive = dto.IsActive,
                RecordId = recordId,
                Category = dto.Category,
                RestrictionValue = dto.RestrictionValue,
                MoneySpent = moneySpent
            };

            _context.Restrictions.Add(restriction);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllRestrictions), new { id = restriction.Id }, new RestrictionDto
            {
                Id = restriction.Id,
                Category = restriction.Category,
                RestrictionValue = restriction.RestrictionValue,
                MoneySpent = restriction.MoneySpent,
                Name = restriction.Name,
                Description = restriction.Description,
                IsActive = restriction.IsActive
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRestriction(int id, RestrictionUpdateDto dto)
        {
            var restriction = await _context.Restrictions.FindAsync(id);
            if (restriction == null) return NotFound();

            restriction.Category = dto.Category ?? restriction.Category;
            restriction.RestrictionValue = dto.RestrictionValue ?? restriction.RestrictionValue;

            var restrictions = await _context.Restrictions
                .Where(r => r.RecordId == restriction.RecordId && r.Category == restriction.Category)
                .ToListAsync();
            var sum = await _context.Transactions
                .Where(t => t.RecordId == restriction.RecordId && t.Category == restriction.Category)
                .SumAsync(t => t.TransactionValue);
            var moneySpent = Math.Abs(sum);
            foreach (var r in restrictions)
                r.MoneySpent = moneySpent;

            await _context.SaveChangesAsync();
            return Ok(new RestrictionDto
            {
                Id = restriction.Id,
                Category = restriction.Category ?? string.Empty,
                RestrictionValue = restriction.RestrictionValue,
                MoneySpent = restriction.MoneySpent,
                Name = restriction.Name ?? string.Empty,
                Description = restriction.Description ?? string.Empty,
                IsActive = restriction.IsActive
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRestriction(int id)
        {
            var restriction = await _context.Restrictions.FindAsync(id);
            if (restriction == null) return NotFound();
            var recordId = restriction.RecordId;
            var category = restriction.Category;
            _context.Restrictions.Remove(restriction);
            await _context.SaveChangesAsync();
            var restrictions = await _context.Restrictions
                .Where(r => r.RecordId == recordId && r.Category == category)
                .ToListAsync();
            var sum = await _context.Transactions
                .Where(t => t.RecordId == recordId && t.Category == category)
                .SumAsync(t => t.TransactionValue);
            var moneySpent = Math.Abs(sum);
            foreach (var r in restrictions)
                r.MoneySpent = moneySpent;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("summary")]
        public async Task<ActionResult<RestrictionSummaryDto>> GetRestrictionSummary()
        {
            var recordId = GetRecordId();
            var restrictions = await _context.Restrictions.Where(r => r.RecordId == recordId).ToListAsync();

            return new RestrictionSummaryDto
            {
                AccountId = recordId,
                TotalRestrictions = restrictions.Count,
                TotalSpent = restrictions.Sum(r => r.MoneySpent),
                ActiveRestrictions = restrictions.Count(r => r.IsActive)
            };
        }

        [HttpGet("report/{recordId}")]
        public async Task<ActionResult<IEnumerable<RestrictionReportDto>>> GetRestrictionReport(int recordId)
        {
            var record = await _context.Records.Include(r => r.Restrictions).FirstOrDefaultAsync(r => r.Id == recordId);
            if (record == null) return NotFound("Record not found");

            var report = record.Restrictions.Select(r => new RestrictionReportDto
            {
                RestrictionId = r.Id,
                Category = r.Category,
                RestrictionValue = r.RestrictionValue,
                MoneySpent = r.MoneySpent
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