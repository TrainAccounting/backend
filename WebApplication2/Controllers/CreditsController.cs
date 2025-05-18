using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trainacc.Filters;
using Trainacc.Services;
using Trainacc.Models;

namespace Trainacc.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(LogActionFilter))]
    [ServiceFilter(typeof(ETagFilter))]
    public class CreditsController : ControllerBase
    {
        private readonly CreditsService _service;
        public CreditsController(CreditsService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CreditDto>>> GetCredits()
        {
            try { return await _service.GetCreditsAsync(); }
            catch { return Problem(); }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CreditDto>> GetCredit(int id)
        {
            try
            {
                var result = await _service.GetCreditAsync(id);
                if (result == null) return NotFound();
                return result;
            }
            catch { return Problem(); }
        }

        [HttpPost]
        public async Task<ActionResult<CreditDto>> CreateCredit(CreditDto dto)
        {
            try
            {
                var created = await _service.CreateCreditAsync(dto);
                return CreatedAtAction(nameof(GetCredit), new { id = created.Id }, created);
            }
            catch { return Problem(); }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCredit(int id, CreditUpdateDto dto)
        {
            try
            {
                var ok = await _service.UpdateCreditAsync(id, dto);
                if (!ok) return NotFound();
                return NoContent();
            }
            catch { return Problem(); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCredit(int id)
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