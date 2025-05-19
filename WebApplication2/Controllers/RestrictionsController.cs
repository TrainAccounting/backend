using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trainacc.Filters;
using Trainacc.Models;
using Trainacc.Services;

namespace Trainacc.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(LogActionFilter))]
    [ServiceFilter(typeof(ETagFilter))]
    public class RestrictionsController : ControllerBase
    {
        private readonly RestrictionsService _service;
        public RestrictionsController(RestrictionsService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RestrictionDto>>> GetRestrictions()
        {
            try { return await _service.GetRestrictionsAsync(); }
            catch { return Problem(); }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RestrictionDto>> GetRestriction(int id)
        {
            try
            {
                var result = await _service.GetRestrictionAsync(id);
                if (result == null) return NotFound();
                return result;
            }
            catch { return Problem(); }
        }

        [HttpPost]
        public async Task<ActionResult<RestrictionDto>> CreateRestriction(RestrictionDto dto)
        {
            try
            {
                var created = await _service.CreateRestrictionAsync(dto);
                return CreatedAtAction(nameof(GetRestriction), new { id = created.Id }, created);
            }
            catch { return Problem(); }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRestriction(int id, RestrictionUpdateDto dto)
        {
            try
            {
                var ok = await _service.UpdateRestrictionAsync(id, dto);
                if (!ok) return NotFound();
                return NoContent();
            }
            catch { return Problem(); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRestriction(int id)
        {
            try
            {
                var ok = await _service.DeleteRestrictionAsync(id);
                if (!ok) return NotFound();
                return NoContent();
            }
            catch { return Problem(); }
        }

        [HttpGet("by-record/{recordId}")]
        public async Task<ActionResult<IEnumerable<RestrictionDto>>> GetRestrictionsByRecord(int recordId)
        {
            try { return await _service.GetRestrictionsByRecordAsync(recordId); }
            catch { return Problem(); }
        }

        [HttpGet("exceeded")]
        public async Task<ActionResult<IEnumerable<RestrictionDto>>> GetExceededRestrictions()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _service.GetExceededRestrictionsAsync(userId);
                return Ok(result);
            }
            catch { return Problem(); }
        }
    }
}