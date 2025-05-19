using System.Collections.Generic;
using System.Threading.Tasks;
using Trainacc.Models;
using Trainacc.Data;
using Microsoft.EntityFrameworkCore;

namespace Trainacc.Services
{
    public class SubscriptionsService
    {
        private readonly AppDbContext _context;
        private readonly TransactionsService _transactionsService;
        public SubscriptionsService(AppDbContext context, TransactionsService transactionsService)
        {
            _context = context;
            _transactionsService = transactionsService;
        }

        public async Task<List<SubscriptionDto>> GetSubscriptionsAsync(int userId)
        {
            var record = await _context.Records.FirstOrDefaultAsync(r => r.UserId == userId);
            if (record == null) return new List<SubscriptionDto>();
            return await _context.Subscriptions.Where(s => s.RecordId == record.Id)
                .Select(s => new SubscriptionDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Amount = s.Amount,
                    Category = s.Category,
                    Type = s.Type,
                    RecordId = s.RecordId,
                    StartDate = s.StartDate,
                    PeriodDays = s.PeriodDays,
                    IsActive = s.IsActive
                }).ToListAsync();
        }

        public async Task<SubscriptionDto> CreateSubscriptionAsync(SubscriptionCreateDto dto)
        {
            var sub = new Subscription
            {
                Name = dto.Name,
                Amount = dto.Amount,
                Category = dto.Category,
                Type = dto.Type,
                RecordId = dto.RecordId,
                StartDate = dto.StartDate,
                PeriodDays = dto.PeriodDays,
                IsActive = true
            };
            _context.Subscriptions.Add(sub);
            await _context.SaveChangesAsync();
            return new SubscriptionDto
            {
                Id = sub.Id,
                Name = sub.Name,
                Amount = sub.Amount,
                Category = sub.Category,
                Type = sub.Type,
                RecordId = sub.RecordId,
                StartDate = sub.StartDate,
                PeriodDays = sub.PeriodDays,
                IsActive = sub.IsActive
            };
        }

        public async Task<int> ProcessActiveSubscriptionsAsync()
        {
            int count = 0;
            var now = DateTime.UtcNow.Date;
            var subs = await _context.Subscriptions.Where(s => s.IsActive).ToListAsync();
            foreach (var sub in subs)
            {
                var lastTx = await _context.Transactions
                    .Where(t => t.RecordId == sub.RecordId && t.Category == sub.Category && t.Type == sub.Type)
                    .OrderByDescending(t => t.TimeOfTransaction)
                    .FirstOrDefaultAsync();
                var lastDate = lastTx?.TimeOfTransaction.Date ?? sub.StartDate.Date.AddDays(-sub.PeriodDays);
                if ((now - lastDate).TotalDays >= sub.PeriodDays)
                {
                    await _transactionsService.CreateTransactionFromSubscriptionAsync(sub);
                    count++;
                }
            }
            return count;
        }
    }
}
