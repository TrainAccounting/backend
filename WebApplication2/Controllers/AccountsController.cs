﻿using Microsoft.AspNetCore.Authorization;
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
    public class AccountsController : ControllerBase
    {
        private readonly AccountsService _service;
        public AccountsController(AccountsService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> Get(
            int? id = null,
            string? mode = null,
            int? recordId = null,
            DateTime? from = null,
            DateTime? to = null,
            int? userId = null
        )
        {
            try
            {
                if (id.HasValue)
                {
                    var result = await _service.GetAccountAsync(id.Value);
                    if (result == null) return NotFound();
                    return Ok(result);
                }

                if (!string.IsNullOrEmpty(mode))
                {
                    switch (mode.ToLower())
                    {
                        case "user-balance":
                            if (!userId.HasValue)
                                return BadRequest("userId обязателен для user-balance");
                            var balance = await _service.GetUserTotalBalanceAsync(userId.Value);
                            return Ok(balance);

                        case "summary":
                            if (!userId.HasValue)
                                return BadRequest("userId обязателен для summary");
                            var summaries = await _service.GetAccountSummariesAsync(userId.Value);
                            return Ok(summaries);

                        case "by-record":
                            if (recordId.HasValue)
                                return Ok(await _service.GetAccountsByRecordAsync(recordId.Value));
                            return BadRequest("recordId required");

                        case "balance-history":
                            if (!userId.HasValue)
                                return BadRequest("userId обязателен для balance-history");
                            if (!from.HasValue || !to.HasValue)
                                return BadRequest("from и to обязательны");
                            var history = await _service.GetBalanceHistoryAsync(userId.Value, from.Value, to.Value);
                            return Ok(history.Select(x => new { date = x.Date, balance = x.Balance }));

                        default:
                            return BadRequest("Unknown mode");
                    }
                }

                return Ok(await _service.GetAccountsAsync());
            }
            catch { return Problem(); }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AccountCreateDto? dto = null, int? recordId = null)
        {
            if (dto == null)
                return BadRequest("Данные не переданы");
            if (!recordId.HasValue)
                return BadRequest("userId обязателен для создания записи");
            try
            {
                var created = await _service.CreateAccountAsync(dto, recordId.Value);
                if (created == null) return NotFound("Record not found");
                return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
            }
            catch { return Problem(); }
        }

        [HttpPut]
        public async Task<IActionResult> Put(int id, [FromBody] AccountUpdateDto dto)
        {
            try
            {
                var ok = await _service.UpdateAccountAsync(id, dto);
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
                var ok = await _service.DeleteAccountAsync(id);
                if (!ok) return NotFound();
                return NoContent();
            }
            catch { return Problem(); }
        }
    }
}