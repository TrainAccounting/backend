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
        public async Task<ActionResult<IEnumerable<RecordDto>>> GetRecords()
        {
            try { return await _service.GetRecordsAsync(); }
            catch { return Problem(); }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RecordDto>> GetRecord(int id)
        {
            try
            {
                var result = await _service.GetRecordAsync(id);
                if (result == null) return NotFound();
                return result;
            }
            catch { return Problem(); }
        }

        [HttpPost]
        public async Task<ActionResult<RecordDto>> CreateRecord(RecordCreateDto recordDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var created = await _service.CreateRecordAsync(recordDto, userId);
                return CreatedAtAction(nameof(GetRecord), new { id = created.Id }, created);
            }
            catch { return Problem(); }
        }

        [HttpGet("by-user/{userId}")]
        public async Task<ActionResult<IEnumerable<RecordDto>>> GetRecordsByUser(int userId)
        {
            try { return await _service.GetRecordsByUserAsync(userId); }
            catch { return Problem(); }
        }
    }
}