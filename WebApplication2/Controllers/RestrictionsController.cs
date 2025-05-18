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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RestrictionDto>>> GetRestrictions()
        {
            return await _context.Restrictions.Select(r => new RestrictionDto
            {
                Id = r.Id,
                Category = r.Category,
                RestrictionValue = r.RestrictionValue,
                MoneySpent = r.MoneySpent,
                Name = r.Name,
                Description = r.Description,
                IsActive = r.IsActive
            }).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RestrictionDto>> GetRestriction(int id)
        {
            var restriction = await _context.Restrictions.FindAsync(id);
            if (restriction == null) return NotFound();
            return new RestrictionDto
            {
                Id = restriction.Id,
                Category = restriction.Category,
                RestrictionValue = restriction.RestrictionValue,
                MoneySpent = restriction.MoneySpent,
                Name = restriction.Name,
                Description = restriction.Description,
                IsActive = restriction.IsActive
            };
        }

        [HttpPost]
        public async Task<ActionResult<RestrictionDto>> CreateRestriction(RestrictionDto dto)
        {
            var restriction = new Restriction
            {
                Name = dto.Name,
                Description = dto.Description,
                IsActive = dto.IsActive,
                RecordId = dto.Id, // предполагается, что RecordId приходит в dto.Id, иначе скорректировать
                Category = dto.Category,
                RestrictionValue = dto.RestrictionValue,
                MoneySpent = dto.MoneySpent
            };
            _context.Restrictions.Add(restriction);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRestriction), new { id = restriction.Id }, dto);
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