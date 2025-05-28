using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trainacc.Filters;
using Trainacc.Services;
using Trainacc.Models;

namespace Trainacc.Controllers
{
    // [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(LogActionFilter))]
    [ServiceFilter(typeof(ETagFilter))]
    public class CreditsController : ControllerBase
    {
        private readonly CreditsService _service;
        public CreditsController(CreditsService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> Get(
            int? id = null,
            string? mode = null,
            int? recordId = null)
        {
            try
            {
                if (id.HasValue)
                {
                    var result = await _service.GetCreditAsync(id.Value);
                    if (result == null) return NotFound();
                    return Ok(result);
                }
                if (!string.IsNullOrEmpty(mode))
                {
                    switch (mode.ToLower())
                    {
                        case "by-record":
                            if (recordId.HasValue)
                                return Ok(await _service.GetCreditsByRecordAsync(recordId.Value));
                            return BadRequest("recordId required");
                        default:
                            return BadRequest("Unknown mode");
                    }
                }
                return Ok(await _service.GetCreditsAsync());
            }
            catch { return Problem(); }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreditDto? dto = null)
        {
            if (dto == null)
                return BadRequest("Данные не переданы");
            try
            {
                var created = await _service.CreateCreditAsync(dto);
                return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
            }
            catch { return Problem(); }
        }

        [HttpPut]
        public async Task<IActionResult> Put(int id, [FromBody] CreditUpdateDto dto)
        {
            try
            {
                var ok = await _service.UpdateCreditAsync(id, dto);
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
                var ok = await _service.DeleteCreditAsync(id);
                if (!ok) return NotFound();
                return NoContent();
            }
            catch { return Problem(); }
        }
    }
}