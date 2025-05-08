using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trainacc.Data;
using Trainacc.Models;
using Trainacc.Filters;

namespace Trainacc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(LogActionFilter))]
    public class RestrictionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RestrictionsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("by-record/{recordId}")]
        public async Task<ActionResult<IEnumerable<RestrictionDto>>> GetRestrictionsByRecord(int recordId)
        {
            return await _context.Restrictions
                .Where(r => r.RecordId == recordId)
                .Select(r => new RestrictionDto
                {
                    Id = r.Id,
                    Category = r.Category,
                    RestrictionValue = r.RestrictionValue,
                    MoneySpent = r.MoneySpent
                })
                .ToListAsync();
        }
    }
}