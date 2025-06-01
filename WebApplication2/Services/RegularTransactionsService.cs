using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Trainacc.Data;
using Trainacc.Models;

namespace Trainacc.Services
{
    public class RegularTransactionsService
    {
        private readonly AppDbContext _context;
        public RegularTransactionsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<RegularTransactionDto>> GetRegularTransactionsAsync()
        {
            return await _context.RegularTransactions.Select(r => new RegularTransactionDto
            {
                Id = r.Id,
                Category = r.Category,
                TransactionValue = r.TransactionValue,
                AccountId = r.AccountId,
                IsAdd = r.IsAdd,
                CreatedAt = r.CreatedAt,
                PeriodDays = r.PeriodDays
            }).ToListAsync();
        }

        public async Task<RegularTransactionDto?> GetRegularTransactionAsync(int id)
        {
            var r = await _context.RegularTransactions.FindAsync(id);
            if (r == null) return null;
            return new RegularTransactionDto
            {
                Id = r.Id,
                Category = r.Category,
                TransactionValue = r.TransactionValue,
                AccountId = r.AccountId,
                IsAdd = r.IsAdd,
                CreatedAt = r.CreatedAt,
                PeriodDays = r.PeriodDays
            };
        }

        public async Task<List<RegularTransactionDto>> GetRegularTransactionsByAccountAsync(int accountId)
        {
            return await _context.RegularTransactions.Where(r => r.AccountId == accountId)
                .Select(r => new RegularTransactionDto
                {
                    Id = r.Id,
                    Category = r.Category,
                    TransactionValue = r.TransactionValue,
                    AccountId = r.AccountId,
                    IsAdd = r.IsAdd,
                    CreatedAt = r.CreatedAt,
                    PeriodDays = r.PeriodDays
                }).ToListAsync();
        }

        public async Task<RegularTransactionDto> CreateRegularTransactionAsync(RegularTransactionCreateDto dto, int accountsId)
        {
            if (dto.TransactionValue < 0)
                throw new Exception("Сумма регулярной операции не может быть отрицательной");
            if (string.IsNullOrWhiteSpace(dto.Category))
                throw new Exception("Категория обязательна");
            if (dto.PeriodDays <= 0)
                throw new Exception("Период должен быть положительным");
            var reg = new RegularTransaction
            {
                Category = dto.Category,
                TransactionValue = dto.TransactionValue,
                AccountId = accountsId,
                IsAdd = dto.IsAdd,
                CreatedAt = dto.CreatedAt,
                PeriodDays = dto.PeriodDays
            };
            _context.RegularTransactions.Add(reg);
            await _context.SaveChangesAsync();
            return new RegularTransactionDto
            {
                Id = reg.Id,
                Category = reg.Category,
                TransactionValue = reg.TransactionValue,
                AccountId = reg.AccountId,
                IsAdd = reg.IsAdd,
                CreatedAt = reg.CreatedAt,
                PeriodDays = reg.PeriodDays
            };
        }

        public async Task<bool> DeleteRegularTransactionAsync(int id)
        {
            var reg = await _context.RegularTransactions.FindAsync(id);
            if (reg == null) return false;
            _context.RegularTransactions.Remove(reg);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task ApplyRegularTransactionsAsync()
        {
            var now = DateTime.UtcNow;
            var regs = await _context.RegularTransactions.ToListAsync();
            foreach (var reg in regs)
            {
                var account = await _context.Accounts.FindAsync(reg.AccountId);
                if (account == null) continue;
                var lastDate = reg.CreatedAt;
                var lastTx = await _context.Transactions
                    .Where(t => t.AccountId == reg.AccountId && t.Description == $"Regular:{reg.Id}")
                    .OrderByDescending(t => t.TimeOfTransaction)
                    .FirstOrDefaultAsync();
                if (lastTx != null)
                    lastDate = lastTx.TimeOfTransaction;
                int maxIterations = 100;
                int iter = 0;
                while ((now - lastDate).TotalDays >= reg.PeriodDays && iter < maxIterations)
                {
                    if (reg.IsAdd)
                    {
                        if (account.Balance + reg.TransactionValue > decimal.MaxValue)
                            break;
                        account.Balance += reg.TransactionValue;
                    }
                    else
                    {
                        if (account.Balance < reg.TransactionValue)
                        {
                            break;
                        }
                        account.Balance -= reg.TransactionValue;
                    }
                    _context.Transactions.Add(new Transactions
                    {
                        Category = reg.Category,
                        TransactionValue = reg.TransactionValue,
                        TimeOfTransaction = lastDate.AddDays(reg.PeriodDays),
                        AccountId = reg.AccountId,
                        IsAdd = reg.IsAdd,
                        Description = $"Regular:{reg.Id}"
                    });
                    lastDate = lastDate.AddDays(reg.PeriodDays);
                    iter++;
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}
