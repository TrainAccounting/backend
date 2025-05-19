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
    public class DepositsController : ControllerBase
    {
        private readonly DepositsService _service;
        public DepositsController(DepositsService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepositDto>>> GetDeposits()
        {
            try { return await _service.GetDepositsAsync(); }
            catch { return Problem(); }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DepositDto>> GetDeposit(int id)
        {
            try
            {
                var result = await _service.GetDepositAsync(id);
                if (result == null) return NotFound();
                return result;
            }
            catch { return Problem(); }
        }

        [HttpPost]
        public async Task<ActionResult<DepositDto>> CreateDeposit(DepositDto dto)
        {
            try
            {
                var created = await _service.CreateDepositAsync(dto);
                return CreatedAtAction(nameof(GetDeposit), new { id = created.Id }, created);
            }
            catch { return Problem(); }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDeposit(int id, DepositUpdateDto dto)
        {
            try
            {
                var ok = await _service.UpdateDepositAsync(id, dto);
                if (!ok) return NotFound();
                return NoContent();
            }
            catch { return Problem(); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeposit(int id)
        {
            try
            {
                var ok = await _service.DeleteDepositAsync(id);
                if (!ok) return NotFound();
                return NoContent();
            }
            catch { return Problem(); }
        }

        [HttpGet("by-record/{recordId}")]
        public async Task<ActionResult<IEnumerable<DepositDto>>> GetDepositsByRecord(int recordId)
        {
            try { return await _service.GetDepositsByRecordAsync(recordId); }
            catch { return Problem(); }
        }
    }
}