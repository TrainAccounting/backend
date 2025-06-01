using System.Collections.Generic;
using System.Threading.Tasks;
using Trainacc.Models;
using Trainacc.Data;
using Microsoft.EntityFrameworkCore;

namespace Trainacc.Services
{
    public class RestrictionsService
    {
        private readonly AppDbContext _context;
        public RestrictionsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<RestrictionDto>> GetRestrictionsAsync()
        {
            return await _context.Restrictions.Select(r => new RestrictionDto
            {
                Id = r.Id,
                Category = r.Category,
                RestrictionValue = r.RestrictionValue,
                MoneySpent = r.MoneySpent
            }).ToListAsync();
        }

        public async Task<RestrictionDto?> GetRestrictionAsync(int id)
        {
            var r = await _context.Restrictions.FindAsync(id);
            if (r == null) return null;
            return new RestrictionDto
            {
                Id = r.Id,
                Category = r.Category,
                RestrictionValue = r.RestrictionValue,
                MoneySpent = r.MoneySpent
            };
        }

        public async Task<RestrictionDto> CreateRestrictionAsync(RestrictionCreateDto dto, int accountsId)
        {
            var account = await _context.Accounts.Include(a => a.Record).FirstOrDefaultAsync(a => a.Id == accountsId);
            if (account == null)
                throw new Exception("Account not found");
            var restriction = new Restriction
            {
                AccountId = accountsId,
                Category = dto.Category,
                RestrictionValue = dto.RestrictionValue,
                MoneySpent = 0
            };
            _context.Restrictions.Add(restriction);
            await _context.SaveChangesAsync();
            return new RestrictionDto
            {
                Id = restriction.Id,
                Category = restriction.Category,
                RestrictionValue = restriction.RestrictionValue,
                MoneySpent = restriction.MoneySpent
            };
        }

        public async Task<bool> UpdateRestrictionAsync(int id, RestrictionUpdateDto dto)
        {
            var restriction = await _context.Restrictions.FindAsync(id);
            if (restriction == null) return false;
            decimal newValue = dto.RestrictionValue ?? restriction.RestrictionValue;
            restriction.Category = dto.Category ?? restriction.Category;
            restriction.RestrictionValue = newValue;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteRestrictionAsync(int id)
        {
            var restriction = await _context.Restrictions.FindAsync(id);
            if (restriction == null) return false;
            _context.Restrictions.Remove(restriction);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<RestrictionDto>> GetRestrictionsByRecordAsync(int recordId)
        {
            return await _context.Restrictions
                .Where(r => r.AccountId == recordId)
                .Select(r => new RestrictionDto
                {
                    Id = r.Id,
                    Category = r.Category,
                    RestrictionValue = r.RestrictionValue,
                    MoneySpent = r.MoneySpent
                })
                .ToListAsync();
        }

        public async Task<List<RestrictionDto>> GetExceededRestrictionsAsync(int userId)
        {
            var accounts = await _context.Accounts.Where(a => a.Record != null && a.Record.UserId == userId).ToListAsync();
            var accountIds = accounts.Select(a => a.Id).ToList();
            return await _context.Restrictions
                .Where(r => accountIds.Contains(r.AccountId) && r.MoneySpent > r.RestrictionValue)
                .Select(r => new RestrictionDto
                {
                    Id = r.Id,
                    Category = r.Category,
                    RestrictionValue = r.RestrictionValue,
                    MoneySpent = r.MoneySpent
                })
                .ToListAsync();
        }

        public async Task<List<RestrictionDto>> GetRestrictionsByAccountAsync(int accountId)
        {
            return await _context.Restrictions
                .Where(r => r.AccountId == accountId)
                .Select(r => new RestrictionDto
                {
                    Id = r.Id,
                    Category = r.Category,
                    RestrictionValue = r.RestrictionValue,
                    MoneySpent = r.MoneySpent
                })
                .ToListAsync();
        }
    }
}
