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
                    MoneySpent = r.MoneySpent
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

        [HttpPost]
        public async Task<IActionResult> CreateRestriction([FromBody] RestrictionDto dto)
        {
            var recordId = GetRecordId();

            var restriction = new Restriction
            {
                Name = dto.Name,
                Description = dto.Description,
                IsActive = dto.IsActive,
                RecordId = recordId
            };

            _context.Restrictions.Add(restriction);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllRestrictions), new { id = restriction.Id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRestriction(int id, RestrictionUpdateDto dto)
        {
            var restriction = await _context.Restrictions.FindAsync(id);
            if (restriction == null) return NotFound();

            restriction.Category = dto.Category ?? restriction.Category;
            restriction.RestrictionValue = dto.RestrictionValue ?? restriction.RestrictionValue;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRestriction(int id)
        {
            var restriction = await _context.Restrictions.FindAsync(id);
            if (restriction == null) return NotFound();

            _context.Restrictions.Remove(restriction);
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
                TotalRestrictions = restrictions.Count,
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