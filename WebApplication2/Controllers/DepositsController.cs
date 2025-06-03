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
    public class DepositsController : ControllerBase
    {
        private readonly DepositsService _service;
        public DepositsController(DepositsService service) => _service = service;

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
                    var result = await _service.GetDepositAsync(id.Value);
                    if (result == null) return NotFound();
                    return Ok(result);
                }
                if (!string.IsNullOrEmpty(mode))
                {
                    switch (mode.ToLower())
                    {
                        case "by-account":
                            if (accountId.HasValue)
                                return Ok(await _service.GetDepositsByAccountAsync(accountId.Value));
                            return BadRequest("accountId required");
                        default:
                            return BadRequest("Unknown mode");
                    }
                }
                return Ok(await _service.GetDepositsAsync());
            }
            catch { return Problem(); }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] DepositCreateDto dto, int accountId, int sourceAccountId)
        {
            if (dto == null)
                return BadRequest("Данные не переданы");
            var created = await _service.CreateDepositAsync(dto, accountId, sourceAccountId);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut]
        public async Task<IActionResult> Put(int id, [FromBody] DepositUpdateDto dto)
        {
            try
            {
                var ok = await _service.UpdateDepositAsync(id, dto);
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
                var ok = await _service.DeleteDepositAsync(id);
                if (!ok) return NotFound();
                return NoContent();
            }
            catch { return Problem(); }
        }

        [HttpPost("close/{depositId}/{accountId}")]
        public async Task<IActionResult> CloseDeposit(int depositId, int accountId, [FromQuery] bool isEarly = false)
        {
            var ok = await _service.CloseDepositAsync(depositId, accountId, isEarly);
            if (!ok) return BadRequest(isEarly ? "Депозит уже закрыт, не найден или досрочное закрытие невозможно" : "Депозит уже закрыт или не найден");
            return Ok(isEarly ? "Депозит досрочно закрыт и деньги переведены на счёт" : "Депозит успешно закрыт и деньги переведены на счёт");
        }
    }
}