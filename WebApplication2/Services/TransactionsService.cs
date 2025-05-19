using System.Collections.Generic;
using System.Threading.Tasks;
using Trainacc.Models;
using Trainacc.Data;
using Microsoft.EntityFrameworkCore;

namespace Trainacc.Services
{
    public class TransactionsService
    {
        private readonly AppDbContext _context;
        private readonly RestrictionsService _restrictionsService;
        public TransactionsService(AppDbContext context, RestrictionsService restrictionsService)
        {
            _context = context;
            _restrictionsService = restrictionsService;
        }

        public async Task<List<TransactionDto>> GetTransactionsAsync()
        {
            return await _context.Transactions.Select(t => new TransactionDto
            {
                Id = t.Id,
                Category = t.Category,
                TransactionValue = t.TransactionValue,
                TimeOfTransaction = t.TimeOfTransaction,
                Type = t.Type
            }).ToListAsync();
        }

        public async Task<TransactionDto?> GetTransactionAsync(int id)
        {
            var t = await _context.Transactions.FindAsync(id);
            if (t == null) return null;
            return new TransactionDto
            {
                Id = t.Id,
                Category = t.Category,
                TransactionValue = t.TransactionValue,
                TimeOfTransaction = t.TimeOfTransaction,
                Type = t.Type
            };
        }

        public async Task<TransactionDto> CreateTransactionAsync(TransactionCreateDto dto)
        {
            var transaction = new Transactions
            {
                Category = dto.Category,
                TransactionValue = dto.TransactionValue,
                TimeOfTransaction = DateTime.UtcNow,
                RecordId = dto.RecordId,
                Type = dto.Type,
                CreatedAt = DateTime.UtcNow
            };
            _context.Transactions.Add(transaction);
            
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.RecordId == dto.RecordId);
            if (account != null)
            {
                if (dto.Type == TransactionType.Income)
                    account.Balance += dto.TransactionValue;
                else
                    account.Balance -= dto.TransactionValue;
            }
            
            if (dto.Type == TransactionType.Expense)
            {
                await _restrictionsService.UpdateRestrictionsSpentAsync(dto.RecordId, dto.Category, dto.TransactionValue);
            }
            await _context.SaveChangesAsync();
            return new TransactionDto
            {
                Id = transaction.Id,
                Category = transaction.Category,
                TransactionValue = transaction.TransactionValue,
                TimeOfTransaction = transaction.TimeOfTransaction,
                Type = transaction.Type
            };
        }

        public async Task<TransactionDto> CreateTransactionAndUpdateBalanceAsync(TransactionCreateDto dto)
        {
            var transaction = new Transactions
            {
                Category = dto.Category,
                TransactionValue = dto.TransactionValue,
                TimeOfTransaction = DateTime.UtcNow,
                RecordId = dto.RecordId
            };
            _context.Transactions.Add(transaction);
            
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.RecordId == dto.RecordId);
            if (account != null)
            {
                account.Balance += dto.TransactionValue;
            }
            await _context.SaveChangesAsync();
            return new TransactionDto
            {
                Id = transaction.Id,
                Category = transaction.Category,
                TransactionValue = transaction.TransactionValue,
                TimeOfTransaction = transaction.TimeOfTransaction
            };
        }

        public async Task<bool> UpdateTransactionAsync(int id, TransactionUpdateDto dto)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return false;
            transaction.UpdatedAt = DateTime.UtcNow;
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.RecordId == transaction.RecordId);
            if (account != null)
            {
                if (transaction.Type == TransactionType.Income)
                    account.Balance -= transaction.TransactionValue;
                else
                    account.Balance += transaction.TransactionValue;
            }
            if (transaction.Type == TransactionType.Expense)
            {
                await _restrictionsService.UpdateRestrictionsSpentAsync(transaction.RecordId, transaction.Category, -transaction.TransactionValue);
            }
            transaction.Category = dto.Category ?? transaction.Category;
            if (dto.TransactionValue.HasValue)
                transaction.TransactionValue = dto.TransactionValue.Value;
            if (dto.Type.HasValue)
                transaction.Type = dto.Type.Value;
            if (account != null)
            {
                if (transaction.Type == TransactionType.Income)
                    account.Balance += transaction.TransactionValue;
                else
                    account.Balance -= transaction.TransactionValue;
            }
            if (transaction.Type == TransactionType.Expense)
            {
                await _restrictionsService.UpdateRestrictionsSpentAsync(transaction.RecordId, transaction.Category, transaction.TransactionValue);
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTransactionAsync(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return false;
            transaction.DeletedAt = DateTime.UtcNow;
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.RecordId == transaction.RecordId);
            if (account != null)
            {
                if (transaction.Type == TransactionType.Income)
                    account.Balance -= transaction.TransactionValue;
                else
                    account.Balance += transaction.TransactionValue;
            }
            if (transaction.Type == TransactionType.Expense)
            {
                await _restrictionsService.UpdateRestrictionsSpentAsync(transaction.RecordId, transaction.Category, -transaction.TransactionValue);
            }
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<TransactionSummaryDto>> GetSummaryByCategoryAsync(int userId, DateTime? from = null, DateTime? to = null)
        {
            var record = await _context.Records.FirstOrDefaultAsync(r => r.UserId == userId);
            if (record == null) return new List<TransactionSummaryDto>();
            var query = _context.Transactions.Where(t => t.RecordId == record.Id);
            if (from.HasValue) query = query.Where(t => t.TimeOfTransaction >= from);
            if (to.HasValue) query = query.Where(t => t.TimeOfTransaction <= to);
            return await query
                .GroupBy(t => new { t.Category, t.Type })
                .Select(g => new TransactionSummaryDto
                {
                    Category = g.Key.Category,
                    Type = g.Key.Type,
                    TotalTransactions = g.Count(),
                    TotalValue = g.Sum(x => x.TransactionValue),
                    TotalAmount = g.Sum(x => x.TransactionValue)
                })
                .ToListAsync();
        }

        public async Task<List<TransactionDto>> GetTransactionsByRecordAsync(int recordId)
        {
            return await _context.Transactions
                .Where(t => t.RecordId == recordId)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    Category = t.Category,
                    TransactionValue = t.TransactionValue,
                    TimeOfTransaction = t.TimeOfTransaction,
                    Type = t.Type
                })
                .ToListAsync();
        }

        public async Task<List<TransactionSummaryDto>> GetTopExpensesByCategoryAsync(int userId, int topN, DateTime? from = null, DateTime? to = null)
        {
            var record = await _context.Records.FirstOrDefaultAsync(r => r.UserId == userId);
            if (record == null) return new List<TransactionSummaryDto>();
            var query = _context.Transactions.Where(t => t.RecordId == record.Id && t.Type == TransactionType.Expense);
            if (from.HasValue) query = query.Where(t => t.TimeOfTransaction >= from);
            if (to.HasValue) query = query.Where(t => t.TimeOfTransaction <= to);
            return await query
                .GroupBy(t => t.Category)
                .Select(g => new TransactionSummaryDto
                {
                    Category = g.Key,
                    Type = TransactionType.Expense,
                    TotalTransactions = g.Count(),
                    TotalValue = g.Sum(x => x.TransactionValue),
                    TotalAmount = g.Sum(x => x.TransactionValue)
                })
                .OrderByDescending(x => x.TotalValue)
                .Take(topN)
                .ToListAsync();
        }

        public async Task<List<TransactionDto>> FilterTransactionsAsync(int userId, TransactionType? type = null, string? category = null, DateTime? from = null, DateTime? to = null, decimal? min = null, decimal? max = null)
        {
            var record = await _context.Records.FirstOrDefaultAsync(r => r.UserId == userId);
            if (record == null) return new List<TransactionDto>();
            var query = _context.Transactions.Where(t => t.RecordId == record.Id);
            if (type.HasValue) query = query.Where(t => t.Type == type);
            if (!string.IsNullOrEmpty(category)) query = query.Where(t => t.Category == category);
            if (from.HasValue) query = query.Where(t => t.TimeOfTransaction >= from);
            if (to.HasValue) query = query.Where(t => t.TimeOfTransaction <= to);
            if (min.HasValue) query = query.Where(t => t.TransactionValue >= min);
            if (max.HasValue) query = query.Where(t => t.TransactionValue <= max);
            return await query.Select(t => new TransactionDto
            {
                Id = t.Id,
                Category = t.Category,
                TransactionValue = t.TransactionValue,
                TimeOfTransaction = t.TimeOfTransaction,
                Type = t.Type
            }).ToListAsync();
        }

        public async Task<int> GetUserRecordId(int userId)
        {
            var record = await _context.Records.FirstOrDefaultAsync(r => r.UserId == userId);
            if (record == null) throw new Exception("Record not found");
            return record.Id;
        }

        public async Task<int> ArchiveOldTransactionsAsync(DateTime beforeDate)
        {
            var oldTxs = await _context.Transactions.Where(t => t.TimeOfTransaction < beforeDate && t.DeletedAt == null).ToListAsync();
            foreach (var tx in oldTxs)
            {
                tx.DeletedAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
            return oldTxs.Count;
        }
    }
}
