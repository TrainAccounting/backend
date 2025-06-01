using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Trainacc.Models;
using Trainacc.Services;
using System;
using System.Collections.Generic;

namespace Trainacc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegularTransactionsController : ControllerBase
    {
        private readonly RegularTransactionsService _service;
        public RegularTransactionsController(RegularTransactionsService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int? id = null, int? accountId = null)
        {
            if (id.HasValue)
            {
                var result = await _service.GetRegularTransactionAsync(id.Value);
                if (result == null) return NotFound();
                return Ok(result);
            }
            if (accountId.HasValue)
                return Ok(await _service.GetRegularTransactionsByAccountAsync(accountId.Value));
            return Ok(await _service.GetRegularTransactionsAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RegularTransactionCreateDto dto, int? accountsId = null)
        {
            if (!accountsId.HasValue)
                return BadRequest("accountId обязателен для создания записи");
            var created = await _service.CreateRegularTransactionAsync(dto, accountsId.Value);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteRegularTransactionAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
