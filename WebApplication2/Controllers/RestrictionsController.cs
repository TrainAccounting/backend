using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trainacc.Filters;
using Trainacc.Models;
using Trainacc.Services;

namespace Trainacc.Controllers
{
    // [Authorize] 
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(LogActionFilter))]
    [ServiceFilter(typeof(ETagFilter))]
    public class RestrictionsController : ControllerBase
    {
        private readonly RestrictionsService _service;
        public RestrictionsController(RestrictionsService service) => _service = service;


        [HttpGet]
        public async Task<IActionResult> Get(
            int? id = null,
            string? mode = null,
            int? recordId = null,
            int? userId = null 
        )
        {
            try
            {
                if (id.HasValue)
                {
                    var result = await _service.GetRestrictionAsync(id.Value);
                    if (result == null) return NotFound();
                    return Ok(result);
                }

                if (!string.IsNullOrEmpty(mode))
                {
                    switch (mode.ToLower())
                    {
                        case "by-record":
                            if (recordId.HasValue)
                                return Ok(await _service.GetRestrictionsByRecordAsync(recordId.Value));
                            return BadRequest("recordId required");
                        case "exceeded":
                            return BadRequest("Режим 'exceeded' не поддерживается");
                        default:
                            return BadRequest("Unknown mode");
                    }
                }

                return Ok(await _service.GetRestrictionsAsync());
            }
            catch { return Problem(); }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RestrictionDto? dto = null)
        {
            if (dto == null)
                return BadRequest("Данные не переданы");
            try
            {
                var created = await _service.CreateRestrictionAsync(dto);
                return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
            }
            catch { return Problem(); }
        }

        [HttpPut]
        public async Task<IActionResult> Put(int id, [FromBody] RestrictionUpdateDto dto)
        {
            try
            {
                var ok = await _service.UpdateRestrictionAsync(id, dto);
                if (!ok) return NotFound();
                return NoContent();
            }
            catch { return Problem(); }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var ok = await _service.DeleteRestrictionAsync(id);
                if (!ok) return NotFound();
                return NoContent();
            }
            catch { return Problem(); }
        }
    }
}