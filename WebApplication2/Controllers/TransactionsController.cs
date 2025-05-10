using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trainacc.Data;
using Trainacc.Models;
using Trainacc.Filters;

namespace Trainacc.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(LogActionFilter))]
    [ServiceFilter(typeof(ETagFilter))]
    public class TransactionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TransactionsController(AppDbContext context) => _context = context;

        [HttpGet("by-record/{recordId}")]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactionsByRecord(int recordId) =>
            await _context.Transactions
                .Where(t => t.RecordId == recordId)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    Category = t.Category,
                    TransactionValue = t.TransactionValue,
                    TimeOfTransaction = t.TimeOfTransaction
                })
                .ToListAsync();

        [HttpPost]
        public async Task<ActionResult<TransactionDto>> CreateTransaction(TransactionCreateDto dto)
        {
            var record = await _context.Records.FindAsync(dto.RecordId);
            if (record == null) return NotFound("Record not found");

            var transaction = new Transactions
            {
                Category = dto.Category,
                TransactionValue = dto.TransactionValue,
                TimeOfTransaction = DateTime.UtcNow,
                RecordId = dto.RecordId
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTransactionsByRecord), new { recordId = dto.RecordId }, new TransactionDto
            {
                Id = transaction.Id,
                Category = transaction.Category,
                TransactionValue = transaction.TransactionValue,
                TimeOfTransaction = transaction.TimeOfTransaction
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(int id, TransactionUpdateDto dto)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return NotFound();

            transaction.Category = dto.Category ?? transaction.Category;
            transaction.TransactionValue = dto.TransactionValue ?? transaction.TransactionValue;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return NotFound();

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}