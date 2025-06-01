using System.Collections.Generic;
using System.Threading.Tasks;
using Trainacc.Models;
using Trainacc.Data;
using Microsoft.EntityFrameworkCore;

namespace Trainacc.Services
{
    public class AccountsService
    {
        private readonly AppDbContext _context;
        public AccountsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<AccountDto>> GetAccountsAsync()
        {
            return await _context.Accounts.Select(a => new AccountDto
            {
                Id = a.Id,
                NameOfAccount = a.NameOfAccount,
                DateOfOpening = a.DateOfOpening,
                Balance = a.Balance
            }).ToListAsync();
        }

        public async Task<AccountDto?> GetAccountAsync(int id)
        {
            var a = await _context.Accounts.FindAsync(id);
            if (a == null) return null;
            return new AccountDto
            {
                Id = a.Id,
                NameOfAccount = a.NameOfAccount ?? string.Empty,
                DateOfOpening = a.DateOfOpening,
                Balance = a.Balance
            };
        }

        public async Task<AccountDto?> CreateAccountAsync(AccountCreateDto dto, int recordId)
        {
            var account = new Account
            {
                NameOfAccount = dto.NameOfAccount,
                DateOfOpening = DateTime.UtcNow,
                RecordId = recordId,
                Balance = 0
            };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            return new AccountDto
            {
                Id = account.Id,
                NameOfAccount = account.NameOfAccount,
                DateOfOpening = account.DateOfOpening,
                Balance = account.Balance
            };
        }

        public async Task<bool> UpdateAccountAsync(int id, AccountUpdateDto dto)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return false;
            if (dto.NameOfAccount != null && dto.NameOfAccount != account.NameOfAccount)
            {
                account.NameOfAccount = dto.NameOfAccount;
            }
            if (dto.Balance.HasValue)
            {
                account.Balance = dto.Balance.Value;
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAccountAsync(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return false;
            if (account.Balance != 0)
                throw new Exception("Нельзя удалить счёт с ненулевым балансом");
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> GetUserTotalBalanceAsync(int userId)
        {
            var record = await _context.Records.FirstOrDefaultAsync(r => r.UserId == userId);
            if (record == null) return 0;
            var accounts = await _context.Accounts.Where(a => a.RecordId == record.Id).ToListAsync();
            decimal total = 0;
            foreach (var acc in accounts)
            {
                total += acc.Balance;
            }
            return total;
        }

        public async Task<List<AccountSummaryDto>> GetAccountSummariesAsync(int userId)
        {
            var record = await _context.Records.FirstOrDefaultAsync(r => r.UserId == userId);
            if (record == null) return new List<AccountSummaryDto>();
            return await _context.Accounts
                .Where(a => a.RecordId == record.Id)
                .Select(a => new AccountSummaryDto
                {
                    Id = a.Id,
                    NameOfAccount = a.NameOfAccount,
                    Balance = a.Balance
                })
                .ToListAsync();
        }

        public async Task<List<AccountDto>> GetAccountsByRecordAsync(int recordId)
        {
            return await _context.Accounts
                .Where(a => a.RecordId == recordId)
                .Select(a => new AccountDto
                {
                    Id = a.Id,
                    NameOfAccount = a.NameOfAccount,
                    DateOfOpening = a.DateOfOpening,
                    Balance = a.Balance
                })
                .ToListAsync();
        }

        public async Task<List<(DateTime Date, decimal Balance)>> GetBalanceHistoryAsync(int userId, DateTime from, DateTime to)
        {
            var accounts = await _context.Accounts.Where(a => a.Record != null && a.Record.UserId == userId).ToListAsync();
            var accountIds = accounts.Select(a => a.Id).ToList();
            var transactions = await _context.Transactions
                .Where(t => accountIds.Contains(t.AccountId) && t.TimeOfTransaction >= from && t.TimeOfTransaction <= to)
                .ToListAsync();
            var history = new List<(DateTime Date, decimal Balance)>();
            decimal balance = 0;
            DateTime? lastDate = null;
            foreach (var t in transactions)
            {
                if (t.IsAdd)
                    balance += t.TransactionValue;
                else
                    balance -= t.TransactionValue;
                if (lastDate == null || t.TimeOfTransaction.Date != lastDate.Value.Date)
                {
                    history.Add((t.TimeOfTransaction.Date, balance));
                    lastDate = t.TimeOfTransaction.Date;
                }
            }
            return history;
        }
    }
}
