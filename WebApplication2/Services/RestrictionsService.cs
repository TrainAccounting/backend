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
                MoneySpent = r.MoneySpent,
                Name = r.Name,
                Description = r.Description,
                IsActive = r.IsActive
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
                MoneySpent = r.MoneySpent,
                Name = r.Name,
                Description = r.Description,
                IsActive = r.IsActive
            };
        }

        public async Task<RestrictionDto> CreateRestrictionAsync(RestrictionDto dto)
        {
            var restriction = new Restriction
            {
                Name = dto.Name,
                Description = dto.Description,
                IsActive = dto.IsActive,
                RecordId = dto.Id,
                Category = dto.Category,
                RestrictionValue = dto.RestrictionValue,
                MoneySpent = dto.MoneySpent
            };
            _context.Restrictions.Add(restriction);
            await _context.SaveChangesAsync();
            dto.Id = restriction.Id;
            return dto;
        }

        public async Task<bool> UpdateRestrictionAsync(int id, RestrictionUpdateDto dto)
        {
            var restriction = await _context.Restrictions.FindAsync(id);
            if (restriction == null) return false;
            restriction.Category = dto.Category ?? restriction.Category;
            restriction.RestrictionValue = dto.RestrictionValue ?? restriction.RestrictionValue;
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
                .Where(r => r.RecordId == recordId)
                .Select(r => new RestrictionDto
                {
                    Id = r.Id,
                    Category = r.Category,
                    RestrictionValue = r.RestrictionValue,
                    MoneySpent = r.MoneySpent,
                    Name = r.Name,
                    Description = r.Description,
                    IsActive = r.IsActive
                })
                .ToListAsync();
        }

        public async Task UpdateRestrictionsSpentAsync(int recordId, string? category, decimal delta)
        {
            var restriction = await _context.Restrictions.FirstOrDefaultAsync(r => r.RecordId == recordId && r.Category == category && r.IsActive);
            if (restriction != null)
            {
                restriction.MoneySpent += delta;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<RestrictionDto>> GetExceededRestrictionsAsync(int userId)
        {
            var record = await _context.Records.FirstOrDefaultAsync(r => r.UserId == userId);
            if (record == null) return new List<RestrictionDto>();
            return await _context.Restrictions
                .Where(r => r.RecordId == record.Id && r.IsActive && r.MoneySpent > r.RestrictionValue)
                .Select(r => new RestrictionDto
                {
                    Id = r.Id,
                    Category = r.Category,
                    RestrictionValue = r.RestrictionValue,
                    MoneySpent = r.MoneySpent,
                    Name = r.Name,
                    Description = r.Description,
                    IsActive = r.IsActive
                })
                .ToListAsync();
        }
    }
}
