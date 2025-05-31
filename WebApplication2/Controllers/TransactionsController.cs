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
            int? recordId = null,
            int? topN = null,
            TransactionType? type = null,
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
                            return Ok(await _service.GetSummaryByCategoryAsync(userId.Value, from, to));
                        case "top":
                            if (!userId.HasValue)
                                return BadRequest("userId обязателен для top");
                            return Ok(await _service.GetTopExpensesByCategoryAsync(userId.Value, topN ?? 5, from, to));
                        case "filter":
                            if (!userId.HasValue)
                                return BadRequest("userId обязателен для filter");
                            return Ok(await _service.FilterTransactionsAsync(userId.Value, type, category, from, to, min, max));
                        case "by-record":
                            if (recordId.HasValue)
                                return Ok(await _service.GetTransactionsByRecordAsync(recordId.Value));
                            return BadRequest("recordId required");
                        case "export":
                            var excelBytes = await _service.ExportTransactionsToExcelAsync(recordId, userId, from, to);
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
        public async Task<IActionResult> Post([FromBody] TransactionCreateDto? dto = null)
        {
            if (dto == null)
                return BadRequest("Данные не переданы");
            if (!Enum.IsDefined(typeof(TransactionType), dto.Type))
                return BadRequest("Некорректный тип транзакции (Type)");
            try
            {
                var created = await _service.CreateTransactionAsync(dto);
                return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
            }
            catch { return Problem(); }
        }

        [HttpPut]
        public async Task<IActionResult> Put(int id, [FromBody] TransactionUpdateDto dto)
        {
            if (dto.Type.HasValue && !Enum.IsDefined(typeof(TransactionType), dto.Type.Value))
                return BadRequest("Некорректный тип транзакции (Type)");
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