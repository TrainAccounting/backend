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

        public async Task<AccountDto?> CreateAccountAsync(AccountCreateDto dto)
        {
            var record = await _context.Records.FindAsync(dto.RecordId);
            if (record == null) return null;
            var account = new Account
            {
                NameOfAccount = dto.NameOfAccount,
                DateOfOpening = DateTime.UtcNow,
                RecordId = dto.RecordId,
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
            account.NameOfAccount = dto.NameOfAccount ?? account.NameOfAccount;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAccountAsync(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return false;
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> GetUserTotalBalanceAsync(int userId)
        {
            var record = await _context.Records.FirstOrDefaultAsync(r => r.UserId == userId);
            if (record == null) return 0;
            return await _context.Accounts.Where(a => a.RecordId == record.Id).SumAsync(a => a.Balance);
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
            var record = await _context.Records.FirstOrDefaultAsync(r => r.UserId == userId);
            if (record == null) return new List<(DateTime, decimal)>();
            var transactions = await _context.Transactions
                .Where(t => t.RecordId == record.Id && t.TimeOfTransaction >= from && t.TimeOfTransaction <= to)
                .OrderBy(t => t.TimeOfTransaction)
                .ToListAsync();
            var result = new List<(DateTime, decimal)>();
            decimal balance = 0;
            DateTime? lastDate = null;
            foreach (var t in transactions)
            {
                if (t.Type == TransactionType.Income)
                    balance += t.TransactionValue;
                else
                    balance -= t.TransactionValue;
                if (lastDate == null || t.TimeOfTransaction.Date != lastDate.Value.Date)
                {
                    result.Add((t.TimeOfTransaction.Date, balance));
                    lastDate = t.TimeOfTransaction.Date;
                }
            }
            return result;
        }
    }
}
