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
                int daysPassed = (int)(now.Date - reg.CreatedAt.Date).TotalDays;
                int daysToCreate = Math.Min(daysPassed, reg.PeriodDays);
                for (int i = 1; i <= daysToCreate; i++)
                {
                    var txTime = reg.CreatedAt.Date.AddDays(i);
                    bool alreadyExists = await _context.Transactions.AnyAsync(t =>
                        t.AccountId == reg.AccountId &&
                        t.Description == $"Regular:{reg.Id}" &&
                        t.TimeOfTransaction == txTime);
                    if (alreadyExists)
                        continue;
                    if (reg.IsAdd)
                    {
                        if (account.Balance + reg.TransactionValue > decimal.MaxValue)
                            continue;
                        account.Balance += reg.TransactionValue;
                    }
                    else
                    {
                        if (account.Balance < reg.TransactionValue)
                            throw new Exception($"[Ошибка] Недостаточно средств для регулярной транзакции (ID={reg.Id}, AccountId={reg.AccountId}, Category={reg.Category}, Value={reg.TransactionValue}, IsAdd={reg.IsAdd})");
                        account.Balance -= reg.TransactionValue;
                    }
                    _context.Transactions.Add(new Transactions
                    {
                        Category = reg.Category,
                        TransactionValue = reg.TransactionValue,
                        TimeOfTransaction = txTime,
                        AccountId = reg.AccountId,
                        IsAdd = reg.IsAdd,
                        Description = $"Regular:{reg.Id}"
                    });
                    if (!reg.IsAdd)
                    {
                        var restriction = await _context.Restrictions.FirstOrDefaultAsync(r => r.AccountId == reg.AccountId && r.Category == reg.Category);
                        if (restriction != null)
                        {
                            restriction.MoneySpent += reg.TransactionValue;
                        }
                    }
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}
