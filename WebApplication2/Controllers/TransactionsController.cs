using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
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
    public class TransactionsController : ControllerBase
    {
        private readonly TransactionsService _service;
        public TransactionsController(TransactionsService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> Get(
            int? id = null,
            string? mode = null,
            int? accountId = null,
            int? topN = null,
            bool? isAdd = null,
            string? category = null,
            DateTime? from = null,
            DateTime? to = null,
            decimal? min = null,
            decimal? max = null,
            int? userId = null
        )
        {
            try
            {
                if (id.HasValue)
                {
                    var result = await _service.GetTransactionAsync(id.Value);
                    if (result == null) return NotFound();
                    return Ok(result);
                }

                if (!string.IsNullOrEmpty(mode))
                {
                    switch (mode.ToLower())
                    {
                        case "summary":
                            if (!userId.HasValue)
                                return BadRequest("userId обязателен для summary");
                            return Ok(await _service.GetSummaryByCategoryAsync(userId.Value, isAdd, category, from, to));
                        case "top":
                            if (!userId.HasValue)
                                return BadRequest("userId обязателен для top");
                            return Ok(await _service.GetTopExpensesByCategoryAsync(userId.Value, topN ?? 5, from, to));
                        case "filter":
                            if (!userId.HasValue)
                                return BadRequest("userId обязателен для filter");
                            return Ok(await _service.FilterTransactionsAsync(userId.Value, isAdd, category, from, to, min, max));
                        case "by-account":
                            if (accountId.HasValue)
                                return Ok(await _service.GetTransactionsByAccountAsync(accountId.Value));
                            return BadRequest("accountId required");
                        case "export":
                            var excelBytes = await _service.ExportTransactionsToExcelByAccountAsync(accountId, userId, from, to);
                            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"transactions_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
                        default:
                            return BadRequest("Unknown mode");
                    }
                }

                return Ok(await _service.GetTransactionsAsync());
            }
            catch { return Problem(); }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TransactionCreateDto? dto = null, int? accountsId = null)
        {
            if (dto == null)
                return BadRequest("Данные не переданы");
            if (!accountsId.HasValue)
                return BadRequest("accountId обязателен для создания записи");
            try
            {
                var created = await _service.CreateTransactionAsync(dto, accountsId.Value);
                return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put(int id, [FromBody] TransactionUpdateDto dto)
        {
            try
            {
                var ok = await _service.UpdateTransactionAsync(id, dto);
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
                var ok = await _service.DeleteTransactionAsync(id);
                if (!ok) return NotFound();
                return NoContent();
            }
            catch { return Problem(); }
        }
    }
}