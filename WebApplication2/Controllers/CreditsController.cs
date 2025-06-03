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
            int? accountId = null)
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
                        case "by-account":
                            if (accountId.HasValue)
                                return Ok(await _service.GetCreditsByAccountAsync(accountId.Value));
                            return BadRequest("accountId required");
                        default:
                            return BadRequest("Unknown mode");
                    }
                }
                return Ok(await _service.GetCreditsAsync());
            }
            catch { return Problem(); }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreditCreateDto dto, int accountId, int togetAccountId)
        {
            if (dto == null)
                return BadRequest("Данные не переданы");
            var created = await _service.CreateCreditAsync(dto, accountId, togetAccountId);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
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

        [HttpPost("close/{creditId}/{accountId}")]
        public async Task<IActionResult> CloseCredit(int creditId, int accountId, [FromQuery] bool isEarly = false)
        {
            var ok = await _service.CloseCreditAsync(creditId, accountId, isEarly);
            if (!ok) return BadRequest(isEarly ? "Кредит уже закрыт, не найден или досрочное погашение невозможно" : "Кредит уже закрыт, не найден или недостаточно средств на счёте");
            return Ok(isEarly ? "Кредит досрочно погашен и закрыт" : "Кредит успешно погашен и закрыт");
        }
    }
}