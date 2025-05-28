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
    public class RecordsController : ControllerBase
    {
        private readonly RecordsService _service;
        public RecordsController(RecordsService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> Get(
            int? id = null,
            string? mode = null,
            int? userId = null)
        {
            try
            {
                if (id.HasValue)
                {
                    var result = await _service.GetRecordAsync(id.Value);
                    if (result == null) return NotFound();
                    return Ok(result);
                }
                if (!string.IsNullOrEmpty(mode))
                {
                    switch (mode.ToLower())
                    {
                        case "by-user":
                            if (userId.HasValue)
                                return Ok(await _service.GetRecordsByUserAsync(userId.Value));
                            return BadRequest("userId required");
                        default:
                            return BadRequest("Unknown mode");
                    }
                }
                return Ok(await _service.GetRecordsAsync());
            }
            catch { return Problem(); }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RecordCreateDto? recordDto = null)
        {
            if (recordDto == null)
                return BadRequest("Данные не переданы");
            try
            {
                int userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var created = await _service.CreateRecordAsync(recordDto, userId);
                return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
            }
            catch { return Problem(); }
        }

        [HttpPut]
        public async Task<IActionResult> Put(int id, [FromBody] RecordCreateDto recordDto)
        {
            if (recordDto == null)
                return BadRequest("Данные не переданы");
            try
            {
                var updated = await _service.UpdateRecordAsync(id, recordDto);
                if (updated == null) return NotFound();
                return Ok(updated);
            }
            catch { return Problem(); }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _service.DeleteRecordAsync(id);
                if (!deleted) return NotFound();
                return Ok(new { deleted = true, id });
            }
            catch { return Problem(); }
        }
    }
}