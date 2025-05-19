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
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactions()
        {
            try { return await _service.GetTransactionsAsync(); }
            catch { return Problem(); }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionDto>> GetTransaction(int id)
        {
            try
            {
                var result = await _service.GetTransactionAsync(id);
                if (result == null) return NotFound();
                return result;
            }
            catch { return Problem(); }
        }

        [HttpPost]
        public async Task<ActionResult<TransactionDto>> CreateTransaction(TransactionCreateDto dto)
        {
            if (!Enum.IsDefined(typeof(TransactionType), dto.Type))
                return BadRequest("Некорректный тип транзакции (Type)");
            try
            {
                var created = await _service.CreateTransactionAsync(dto);
                return CreatedAtAction(nameof(GetTransaction), new { id = created.Id }, created);
            }
            catch { return Problem(); }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(int id, TransactionUpdateDto dto)
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            try
            {
                var ok = await _service.DeleteTransactionAsync(id);
                if (!ok) return NotFound();
                return NoContent();
            }
            catch { return Problem(); }
        }

        [HttpGet("summary-by-category")]
        public async Task<ActionResult<List<TransactionSummaryDto>>> GetSummaryByCategory([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _service.GetSummaryByCategoryAsync(userId, from, to);
                return Ok(result);
            }
            catch { return Problem(); }
        }

        [HttpGet("by-record/{recordId}")]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactionsByRecord(int recordId)
        {
            try { return await _service.GetTransactionsByRecordAsync(recordId); }
            catch { return Problem(); }
        }

        [HttpGet("top-expenses")]
        public async Task<ActionResult<List<TransactionSummaryDto>>> GetTopExpenses([FromQuery] int topN = 5, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _service.GetTopExpensesByCategoryAsync(userId, topN, from, to);
                return Ok(result);
            }
            catch { return Problem(); }
        }

        [HttpGet("filter")]
        public async Task<ActionResult<List<TransactionDto>>> FilterTransactions([FromQuery] TransactionType? type, [FromQuery] string? category, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] decimal? min, [FromQuery] decimal? max)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _service.FilterTransactionsAsync(userId, type, category, from, to, min, max);
                return Ok(result);
            }
            catch { return Problem(); }
        }

        [HttpGet("export-csv")]
        public async Task<IActionResult> ExportTransactionsToCsv([FromQuery] TransactionType? type, [FromQuery] string? category, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var transactions = await _service.FilterTransactionsAsync(userId, type, category, from, to, null, null);
                var csv = "Id,Category,Value,Date,Type\n" + string.Join("\n", transactions.Select(t => $"{t.Id},{t.Category},{t.TransactionValue},{t.TimeOfTransaction:yyyy-MM-dd},{t.Type}"));
                var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
                return File(bytes, "text/csv", $"transactions_{DateTime.Now:yyyyMMddHHmmss}.csv");
            }
            catch { return Problem(); }
        }

        [HttpPost("import-csv")]
        public async Task<IActionResult> ImportTransactionsFromCsv()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var file = Request.Form.Files.FirstOrDefault();
                if (file == null || file.Length == 0)
                    return BadRequest("Файл не загружен");
                using var reader = new StreamReader(file.OpenReadStream());
                var lines = new List<string>();
                while (!reader.EndOfStream)
                    lines.Add(await reader.ReadLineAsync() ?? "");
                var recordId = await _service.GetUserRecordId(userId);
                foreach (var line in lines.Skip(1))
                {
                    var parts = line.Split(',');
                    if (parts.Length < 5) continue;
                    var dto = new TransactionCreateDto
                    {
                        Category = parts[1],
                        TransactionValue = decimal.TryParse(parts[2], out var v) ? v : 0,
                        RecordId = recordId,
                        Type = Enum.TryParse<TransactionType>(parts[4], out var t) ? t : TransactionType.Expense
                    };
                    await _service.CreateTransactionAsync(dto);
                }
                return Ok("Импорт завершён");
            }
            catch { return Problem(); }
        }

        [HttpPost("archive")]
        public async Task<IActionResult> ArchiveOldTransactions([FromQuery] DateTime before)
        {
            var count = await _service.ArchiveOldTransactionsAsync(before);
            return Ok($"В архив отправлено: {count} транзакций");
        }
    }
}