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
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.RecordId == dto.RecordId);
            if (account == null)
                throw new Exception("Счёт не найден");
            if (dto.Type == TransactionType.Expense && account.Balance < dto.TransactionValue)
                throw new Exception("Недостаточно средств на счёте для расхода");
            if (dto.TransactionValue < 0)
                throw new Exception("Сумма операции не может быть отрицательной");
            if (string.IsNullOrWhiteSpace(dto.Category))
                throw new Exception("Категория обязательна");
            var t = new Transactions
            {
                Category = dto.Category,
                TransactionValue = dto.TransactionValue,
                TimeOfTransaction = DateTime.UtcNow,
                RecordId = dto.RecordId,
                Type = dto.Type,
                CreatedAt = DateTime.UtcNow,
                Description = dto.Description
            };
            _context.Transactions.Add(t);
            if (dto.Type == TransactionType.Income)
                account.Balance += dto.TransactionValue;
            else
                account.Balance -= dto.TransactionValue;
            await _context.SaveChangesAsync();
            return new TransactionDto
            {
                Id = t.Id,
                Category = t.Category,
                TransactionValue = t.TransactionValue,
                TimeOfTransaction = t.TimeOfTransaction,
                Type = t.Type
            };
        }

        public async Task<bool> UpdateTransactionAsync(int id, TransactionUpdateDto dto)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return false;
            var oldType = transaction.Type;
            var oldValue = transaction.TransactionValue;
            var oldCategory = transaction.Category;
            transaction.UpdatedAt = DateTime.UtcNow;
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.RecordId == transaction.RecordId);
            if (account != null)
            {
                if (oldType == TransactionType.Income)
                    account.Balance -= oldValue;
                else
                    account.Balance += oldValue;
            }
            if (dto.Category != null)
                transaction.Category = dto.Category;
            if (dto.TransactionValue.HasValue)
                transaction.TransactionValue = dto.TransactionValue.Value;
            if (dto.Type.HasValue)
                transaction.Type = dto.Type.Value;
            if (dto.Description != null)
                transaction.Description = dto.Description;
            if (string.IsNullOrWhiteSpace(transaction.Category) || transaction.TransactionValue < 0)
                return false;
            if (account != null)
            {
                if (transaction.Type == TransactionType.Income)
                    account.Balance += transaction.TransactionValue;
                else
                    account.Balance -= transaction.TransactionValue;
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTransactionAsync(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return false;
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.RecordId == transaction.RecordId);
            if (account != null)
            {
                if (transaction.Type == TransactionType.Income)
                    account.Balance -= transaction.TransactionValue;
                else
                    account.Balance += transaction.TransactionValue;
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
                    TotalValue = g.Sum(x => x.TransactionValue)
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
                    TotalValue = g.Sum(x => x.TransactionValue)
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
    }
}
