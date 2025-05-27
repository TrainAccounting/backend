using System.Collections.Generic;
using System.Threading.Tasks;
using Trainacc.Models;
using Trainacc.Data;
using Microsoft.EntityFrameworkCore;

namespace Trainacc.Services
{
    public class RecordsService
    {
        private readonly AppDbContext _context;
        public RecordsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<RecordDto>> GetRecordsAsync()
        {
            return await _context.Records.Include(r => r.User).Select(r => new RecordDto
            {
                Id = r.Id,
                NameOfRecord = r.NameOfRecord,
                DateOfCreation = r.DateOfCreation,
                User = r.User != null ? new UserDto
                {
                    Id = r.User.Id,
                    FIO = r.User.FIO,
                    Email = r.User.Email,
                    Phone = r.User.Phone
                } : null
            }).ToListAsync();
        }

        public async Task<RecordDto?> GetRecordAsync(int id)
        {
            var r = await _context.Records.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);
            if (r == null) return null;
            return new RecordDto
            {
                Id = r.Id,
                NameOfRecord = r.NameOfRecord,
                DateOfCreation = r.DateOfCreation,
                User = r.User != null ? new UserDto
                {
                    Id = r.User.Id,
                    FIO = r.User.FIO,
                    Email = r.User.Email,
                    Phone = r.User.Phone
                } : null
            };
        }

        public async Task<RecordDto> CreateRecordAsync(RecordCreateDto recordDto, int userId)
        {
            var record = new Record
            {
                NameOfRecord = recordDto.NameOfRecord,
                DateOfCreation = DateTime.UtcNow,
                UserId = userId
            };
            _context.Records.Add(record);
            await _context.SaveChangesAsync();
            return new RecordDto
            {
                Id = record.Id,
                NameOfRecord = record.NameOfRecord,
                DateOfCreation = record.DateOfCreation
            };
        }

        public async Task<List<RecordDto>> GetRecordsByUserAsync(int userId)
        {
            return await _context.Records
                .Where(r => r.UserId == userId)
                .Include(r => r.User)
                .Select(r => new RecordDto
                {
                    Id = r.Id,
                    NameOfRecord = r.NameOfRecord,
                    DateOfCreation = r.DateOfCreation,
                    User = r.User != null ? new UserDto
                    {
                        Id = r.User.Id,
                        FIO = r.User.FIO,
                        Email = r.User.Email,
                        Phone = r.User.Phone
                    } : null
                })
                .ToListAsync();
        }

        public async Task<(decimal safetyPillow, decimal savingsGoal, string? savingsGoalName, bool notify)> ProcessEndOfMonthAsync(int recordId)
        {
            var record = await _context.Records.Include(r => r.Restrictions).Include(r => r.Transactions).FirstOrDefaultAsync(r => r.Id == recordId);
            if (record == null) throw new Exception("Record not found");
            var restrictions = record.Restrictions;
            var transactions = record.Transactions.Where(t => t.TimeOfTransaction.Month == DateTime.Now.Month && t.TimeOfTransaction.Year == DateTime.Now.Year).ToList();
            decimal overLimit = 0;
            decimal totalSpent = 0;
            foreach (var restriction in restrictions)
            {
                var spent = transactions.Where(t => t.Category == restriction.Category).Sum(t => t.TransactionValue);
                totalSpent += spent;
                if (spent > restriction.RestrictionValue)
                    overLimit += spent - restriction.RestrictionValue;
            }
            var sumRestrictions = restrictions.Sum(r => r.RestrictionValue);
            decimal savings = record.MonthlySalary - sumRestrictions;
            if (overLimit == 0)
                record.Savings += savings;
            else
                record.Savings += Math.Max(0, savings - overLimit);
            if (record.Savings > 0)
            {
                record.SafetyPillow += record.Savings;
                record.Savings = 0;
            }
            await _context.SaveChangesAsync();
            bool notify = false;
            if (record.SavingsGoal > 0 && record.SafetyPillow >= record.SavingsGoal * 0.25m && record.SafetyPillow < record.SavingsGoal)
            {
                notify = true;
            }
            return (record.SafetyPillow, record.SavingsGoal, record.SavingsGoalName, notify);
        }
    }
}