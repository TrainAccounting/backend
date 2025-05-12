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

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetAllTransactions()
        {
            var recordId = GetRecordId();
            return await _context.Transactions
                .Where(t => t.RecordId == recordId)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    Category = t.Category,
                    TransactionValue = t.TransactionValue,
                    TimeOfTransaction = t.TimeOfTransaction
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionDto>> GetTransaction(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return NotFound();
            return new TransactionDto
            {
                Id = transaction.Id,
                Category = transaction.Category ?? string.Empty,
                TransactionValue = transaction.TransactionValue,
                TimeOfTransaction = transaction.TimeOfTransaction
            };
        }

        [HttpPost]
        public async Task<ActionResult<TransactionDto>> CreateTransaction(TransactionCreateDto dto)
        {
            var recordId = GetRecordId();
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.RecordId == recordId);
            if (account == null)
                return BadRequest("Account not found for this record");

            var absValue = Math.Abs(dto.TransactionValue);
            var transaction = new Transactions
            {
                Category = dto.Category,
                TransactionValue = dto.TransactionValue,
                TimeOfTransaction = DateTime.UtcNow,
                RecordId = recordId
            };
            _context.Transactions.Add(transaction);
            account.Balance += dto.TransactionValue;
            await _context.SaveChangesAsync();

            var restrictions = await _context.Restrictions
                .Where(r => r.RecordId == recordId && r.Category == dto.Category)
                .ToListAsync();
            var moneySpent = await _context.Transactions
                .Where(t => t.RecordId == recordId && t.Category == dto.Category)
                .SumAsync(t => Math.Abs(t.TransactionValue));
            foreach (var r in restrictions)
                r.MoneySpent = moneySpent;
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllTransactions), new { id = transaction.Id }, new TransactionDto
            {
                Id = transaction.Id,
                Category = transaction.Category ?? string.Empty,
                TransactionValue = transaction.TransactionValue,
                TimeOfTransaction = transaction.TimeOfTransaction
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(int id, TransactionUpdateDto dto)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return NotFound();

            var oldValue = Math.Abs(transaction.TransactionValue);
            var oldCategory = transaction.Category;
            var recordId = transaction.RecordId;

            transaction.Category = dto.Category ?? transaction.Category;
            transaction.TransactionValue = dto.TransactionValue ?? transaction.TransactionValue;
            var newValue = Math.Abs(transaction.TransactionValue);

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.RecordId == recordId);
            if (account != null)
            {
                account.Balance = account.Balance - oldValue + newValue;
            }

            var restrictions = await _context.Restrictions
                .Where(r => r.RecordId == recordId && r.Category == transaction.Category)
                .ToListAsync();
            var moneySpent = await _context.Transactions
                .Where(t => t.RecordId == recordId && t.Category == transaction.Category)
                .SumAsync(t => Math.Abs(t.TransactionValue));
            foreach (var r in restrictions)
                r.MoneySpent = moneySpent;

            if (oldCategory != transaction.Category)
            {
                var oldRestrictions = await _context.Restrictions
                    .Where(r => r.RecordId == recordId && r.Category == oldCategory)
                    .ToListAsync();
                var oldMoneySpent = await _context.Transactions
                    .Where(t => t.RecordId == recordId && t.Category == oldCategory)
                    .SumAsync(t => Math.Abs(t.TransactionValue));
                foreach (var r in oldRestrictions)
                    r.MoneySpent = oldMoneySpent;
            }

            await _context.SaveChangesAsync();
            return Ok(new TransactionDto
            {
                Id = transaction.Id,
                Category = transaction.Category ?? string.Empty,
                TransactionValue = transaction.TransactionValue,
                TimeOfTransaction = transaction.TimeOfTransaction
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return NotFound();
            var recordId = transaction.RecordId;
            var category = transaction.Category;
            var value = Math.Abs(transaction.TransactionValue);
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.RecordId == recordId);
            if (account != null)
            {
                account.Balance -= value;
            }
            var restrictions = await _context.Restrictions
                .Where(r => r.RecordId == recordId && r.Category == category)
                .ToListAsync();
            var moneySpent = await _context.Transactions
                .Where(t => t.RecordId == recordId && t.Category == category)
                .SumAsync(t => Math.Abs(t.TransactionValue));
            foreach (var r in restrictions)
                r.MoneySpent = moneySpent;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("summary")]
        public async Task<ActionResult<TransactionSummaryDto>> GetTransactionSummary()
        {
            var recordId = GetRecordId();
            var transactions = await _context.Transactions.Where(t => t.RecordId == recordId).ToListAsync();

            return new TransactionSummaryDto
            {
                TotalTransactions = transactions.Count,
                TotalAmount = transactions.Sum(t => t.TransactionValue)
            };
        }

        [HttpGet("report/{recordId}")]
        public async Task<ActionResult<IEnumerable<TransactionReportDto>>> GetTransactionReport(int recordId)
        {
            var record = await _context.Records.Include(r => r.Transactions).FirstOrDefaultAsync(r => r.Id == recordId);
            if (record == null) return NotFound("Record not found");

            var report = record.Transactions.Select(t => new TransactionReportDto
            {
                TransactionId = t.Id,
                Category = t.Category,
                Value = t.TransactionValue,
                Date = t.TimeOfTransaction
            }).ToList();

            return Ok(report);
        }

        private int GetRecordId()
        {
            var recordIdClaim = User.FindFirst("RecordId");
            if (recordIdClaim == null)
            {
                throw new NullReferenceException("RecordId claim is missing.");
            }
            return int.Parse(recordIdClaim.Value);
        }
    }
}using Microsoft.AspNetCore.Authorization;
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

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetAllTransactions()
        {
            var recordId = GetRecordId();
            return await _context.Transactions
                .Where(t => t.RecordId == recordId)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    Category = t.Category,
                    TransactionValue = t.TransactionValue,
                    TimeOfTransaction = t.TimeOfTransaction
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionDto>> GetTransaction(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return NotFound();
            return new TransactionDto
            {
                Id = transaction.Id,
                Category = transaction.Category ?? string.Empty,
                TransactionValue = transaction.TransactionValue,
                TimeOfTransaction = transaction.TimeOfTransaction
            };
        }

        [HttpPost]
        public async Task<ActionResult<TransactionDto>> CreateTransaction(TransactionCreateDto dto)
        {
            var recordId = GetRecordId();
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.RecordId == recordId);
            if (account == null)
                return BadRequest("Account not found for this record");

            var absValue = Math.Abs(dto.TransactionValue);
            var transaction = new Transactions
            {
                Category = dto.Category,
                TransactionValue = dto.TransactionValue,
                TimeOfTransaction = DateTime.UtcNow,
                RecordId = recordId
            };
            _context.Transactions.Add(transaction);
            account.Balance += dto.TransactionValue;
            await _context.SaveChangesAsync();

            var restrictions = await _context.Restrictions
                .Where(r => r.RecordId == recordId && r.Category == dto.Category)
                .ToListAsync();
            var moneySpent = await _context.Transactions
                .Where(t => t.RecordId == recordId && t.Category == dto.Category)
                .SumAsync(t => Math.Abs(t.TransactionValue));
            foreach (var r in restrictions)
                r.MoneySpent = moneySpent;
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllTransactions), new { id = transaction.Id }, new TransactionDto
            {
                Id = transaction.Id,
                Category = transaction.Category ?? string.Empty,
                TransactionValue = transaction.TransactionValue,
                TimeOfTransaction = transaction.TimeOfTransaction
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(int id, TransactionUpdateDto dto)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return NotFound();

            var oldValue = Math.Abs(transaction.TransactionValue);
            var oldCategory = transaction.Category;
            var recordId = transaction.RecordId;

            transaction.Category = dto.Category ?? transaction.Category;
            transaction.TransactionValue = dto.TransactionValue ?? transaction.TransactionValue;
            var newValue = Math.Abs(transaction.TransactionValue);

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.RecordId == recordId);
            if (account != null)
            {
                account.Balance = account.Balance - oldValue + newValue;
            }

            var restrictions = await _context.Restrictions
                .Where(r => r.RecordId == recordId && r.Category == transaction.Category)
                .ToListAsync();
            var moneySpent = await _context.Transactions
                .Where(t => t.RecordId == recordId && t.Category == transaction.Category)
                .SumAsync(t => Math.Abs(t.TransactionValue));
            foreach (var r in restrictions)
                r.MoneySpent = moneySpent;

            if (oldCategory != transaction.Category)
            {
                var oldRestrictions = await _context.Restrictions
                    .Where(r => r.RecordId == recordId && r.Category == oldCategory)
                    .ToListAsync();
                var oldMoneySpent = await _context.Transactions
                    .Where(t => t.RecordId == recordId && t.Category == oldCategory)
                    .SumAsync(t => Math.Abs(t.TransactionValue));
                foreach (var r in oldRestrictions)
                    r.MoneySpent = oldMoneySpent;
            }

            await _context.SaveChangesAsync();
            return Ok(new TransactionDto
            {
                Id = transaction.Id,
                Category = transaction.Category ?? string.Empty,
                TransactionValue = transaction.TransactionValue,
                TimeOfTransaction = transaction.TimeOfTransaction
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return NotFound();
            var recordId = transaction.RecordId;
            var category = transaction.Category;
            var value = Math.Abs(transaction.TransactionValue);
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.RecordId == recordId);
            if (account != null)
            {
                account.Balance -= value;
            }
            var restrictions = await _context.Restrictions
                .Where(r => r.RecordId == recordId && r.Category == category)
                .ToListAsync();
            var moneySpent = await _context.Transactions
                .Where(t => t.RecordId == recordId && t.Category == category)
                .SumAsync(t => Math.Abs(t.TransactionValue));
            foreach (var r in restrictions)
                r.MoneySpent = moneySpent;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("summary")]
        public async Task<ActionResult<TransactionSummaryDto>> GetTransactionSummary()
        {
            var recordId = GetRecordId();
            var transactions = await _context.Transactions.Where(t => t.RecordId == recordId).ToListAsync();

            return new TransactionSummaryDto
            {
                TotalTransactions = transactions.Count,
                TotalAmount = transactions.Sum(t => t.TransactionValue)
            };
        }

        [HttpGet("report/{recordId}")]
        public async Task<ActionResult<IEnumerable<TransactionReportDto>>> GetTransactionReport(int recordId)
        {
            var record = await _context.Records.Include(r => r.Transactions).FirstOrDefaultAsync(r => r.Id == recordId);
            if (record == null) return NotFound("Record not found");

            var report = record.Transactions.Select(t => new TransactionReportDto
            {
                TransactionId = t.Id,
                Category = t.Category,
                Value = t.TransactionValue,
                Date = t.TimeOfTransaction
            }).ToList();

            return Ok(report);
        }

        private int GetRecordId()
        {
            var recordIdClaim = User.FindFirst("RecordId");
            if (recordIdClaim == null)
            {
                throw new NullReferenceException("RecordId claim is missing.");
            }
            return int.Parse(recordIdClaim.Value);
        }
    }
}