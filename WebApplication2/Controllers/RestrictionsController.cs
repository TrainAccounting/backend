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

        [HttpPost]
        public async Task<ActionResult<RestrictionDto>> CreateRestriction(RestrictionCreateDto dto)
        {
            var record = await _context.Records.FindAsync(dto.RecordId);
            if (record == null) return NotFound("Record not found");

            var restriction = new Restriction
            {
                Category = dto.Category,
                RestrictionValue = dto.RestrictionValue,
                MoneySpent = 0,
                RecordId = dto.RecordId
            };

            _context.Restrictions.Add(restriction);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRestrictionsByRecord), new { recordId = dto.RecordId }, new RestrictionDto
            {
                Id = restriction.Id,
                Category = restriction.Category,
                RestrictionValue = restriction.RestrictionValue,
                MoneySpent = restriction.MoneySpent
            });
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
    }
}