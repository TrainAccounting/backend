using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
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
            decimal? max = null)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
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
                            return Ok(await _service.GetSummaryByCategoryAsync(userId, from, to));
                        case "top":
                            return Ok(await _service.GetTopExpensesByCategoryAsync(userId, topN ?? 5, from, to));
                        case "filter":
                            return Ok(await _service.FilterTransactionsAsync(userId, type, category, from, to, min, max));
                        case "by-record":
                            if (recordId.HasValue)
                                return Ok(await _service.GetTransactionsByRecordAsync(recordId.Value));
                            return BadRequest("recordId required");
                        case "export":
                            var transactions = await _service.FilterTransactionsAsync(userId, type, category, from, to, null, null);
                            var csv = "Id,Category,Value,Date,Type\n" + string.Join("\n", transactions.Select(t => $"{t.Id},{t.Category},{t.TransactionValue},{t.TimeOfTransaction:yyyy-MM-dd},{t.Type}"));
                            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
                            return File(bytes, "text/csv", $"transactions_{DateTime.Now:yyyyMMddHHmmss}.csv");
                        default:
                            return BadRequest("Unknown mode");
                    }
                }
                return Ok(await _service.GetTransactionsAsync());
            }
            catch { return Problem(); }
        }

        [HttpPost]
        public async Task<IActionResult> Post(
            [FromBody] TransactionCreateDto? dto = null,
            string? mode = null)
        {
            if (!string.IsNullOrEmpty(mode))
            {
                switch (mode.ToLower())
                {
                    case "import":
                        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                        var file = Request.Form.Files.FirstOrDefault();
                        if (file == null || file.Length == 0)
                            return BadRequest("Файл не загружен");
                        using (var reader = new StreamReader(file.OpenReadStream()))
                        {
                            var lines = new List<string>();
                            while (!reader.EndOfStream)
                                lines.Add(await reader.ReadLineAsync() ?? "");
                            var recordId = await _service.GetUserRecordId(userId);
                            foreach (var line in lines.Skip(1))
                            {
                                var parts = line.Split(',');
                                if (parts.Length < 5) continue;
                                var importDto = new TransactionCreateDto
                                {
                                    Category = parts[1],
                                    TransactionValue = decimal.TryParse(parts[2], out var v) ? v : 0,
                                    RecordId = recordId,
                                    Type = Enum.TryParse<TransactionType>(parts[4], out var t) ? t : TransactionType.Expense
                                };
                                await _service.CreateTransactionAsync(importDto);
                            }
                        }
                        return Ok("Импорт завершён");
                    case "archive":
                        if (!Request.Query.ContainsKey("before"))
                            return BadRequest("before required");
                        if (!DateTime.TryParse(Request.Query["before"], out var before))
                            return BadRequest("Некорректная дата before");
                        var count = await _service.ArchiveOldTransactionsAsync(before);
                        return Ok($"В архив отправлено: {count} транзакций");
                    default:
                        return BadRequest("Unknown mode");
                }
            }
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