using System.Collections.Generic;
using System.Threading.Tasks;
using Trainacc.Models;
using Trainacc.Data;
using Microsoft.EntityFrameworkCore;

namespace Trainacc.Services
{
    public class CreditsService
    {
        private readonly AppDbContext _context;
        public CreditsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CreditDto>> GetCreditsAsync()
        {
            return await _context.Credits.Select(c => new CreditDto
            {
                Id = c.Id,
                NameOfCredit = c.NameOfCredit,
                CreditStartValue = c.CreditStartValue,
                CreditCurrentValue = c.CreditCurrentValue,
                DateOfOpening = c.DateOfOpening,
                PeriodOfPayment = c.PeriodOfPayment,
                InterestRate = c.InterestRate,
                RecordId = c.RecordId
            }).ToListAsync();
        }

        public async Task<CreditDto?> GetCreditAsync(int id)
        {
            var c = await _context.Credits.FindAsync(id);
            if (c == null) return null;
            return new CreditDto
            {
                Id = c.Id,
                NameOfCredit = c.NameOfCredit,
                CreditStartValue = c.CreditStartValue,
                CreditCurrentValue = c.CreditCurrentValue,
                DateOfOpening = c.DateOfOpening,
                PeriodOfPayment = c.PeriodOfPayment,
                InterestRate = c.InterestRate,
                RecordId = c.RecordId
            };
        }

        public async Task<CreditDto> CreateCreditAsync(CreditDto dto)
        {
            if (dto.InterestRate < 0)
                throw new Exception("Ставка кредита должна быть положительной");
            if (dto.CreditStartValue <= 0)
                throw new Exception("Сумма кредита должна быть положительной");
            var credit = new Credit
            {
                NameOfCredit = dto.NameOfCredit,
                CreditStartValue = dto.CreditStartValue,
                CreditCurrentValue = dto.CreditStartValue,
                DateOfOpening = dto.DateOfOpening,
                PeriodOfPayment = dto.PeriodOfPayment,
                InterestRate = dto.InterestRate,
                RecordId = dto.RecordId,
                IsActive = true
            };
            _context.Credits.Add(credit);
            await _context.SaveChangesAsync();
            dto.Id = credit.Id;
            return dto;
        }

        public async Task<bool> UpdateCreditAsync(int id, CreditUpdateDto dto)
        {
            var credit = await _context.Credits.FindAsync(id);
            if (credit == null) return false;
            credit.NameOfCredit = dto.NameOfCredit ?? credit.NameOfCredit;
            credit.CreditCurrentValue = dto.CreditCurrentValue ?? credit.CreditCurrentValue;
            credit.PeriodOfPayment = dto.PeriodOfPayment ?? credit.PeriodOfPayment;
            credit.InterestRate = dto.InterestRate ?? credit.InterestRate;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCreditAsync(int id)
        {
            var credit = await _context.Credits.FindAsync(id);
            if (credit == null) return false;
            _context.Credits.Remove(credit);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<CreditDto>> GetCreditsByRecordAsync(int recordId)
        {
            return await _context.Credits
                .Where(c => c.RecordId == recordId)
                .Select(c => new CreditDto
                {
                    Id = c.Id,
                    NameOfCredit = c.NameOfCredit,
                    CreditStartValue = c.CreditStartValue,
                    CreditCurrentValue = c.CreditCurrentValue,
                    DateOfOpening = c.DateOfOpening,
                    PeriodOfPayment = c.PeriodOfPayment,
                    InterestRate = c.InterestRate,
                    RecordId = c.RecordId
                })
                .ToListAsync();
        }

        private void AddPenaltyHistory(Credit credit, decimal penalty, string reason)
        {
            var history = new List<CreditPenaltyHistoryItem>();
            if (!string.IsNullOrEmpty(credit.PenaltyHistoryJson))
            {
                try { history = System.Text.Json.JsonSerializer.Deserialize<List<CreditPenaltyHistoryItem>>(credit.PenaltyHistoryJson) ?? new(); }
                catch { history = new List<CreditPenaltyHistoryItem>(); }
            }
            history.Add(new CreditPenaltyHistoryItem
            {
                Date = DateTime.UtcNow,
                PenaltyAmount = penalty,
                Reason = reason
            });
            credit.PenaltyHistoryJson = System.Text.Json.JsonSerializer.Serialize(history);
        }

        public async Task ProcessMonthlyCreditsAsync()
        {
            var now = DateTime.UtcNow;
            var credits = await _context.Credits.Where(c => c.IsActive).ToListAsync();
            foreach (var credit in credits)
            {
                if (credit.PeriodOfPayment <= 0 || credit.CreditStartValue <= 0) continue;
                int monthsPassed = ((now.Year - credit.DateOfOpening.Year) * 12) + now.Month - credit.DateOfOpening.Month;
                if (monthsPassed >= credit.PeriodOfPayment)
                {
                    credit.IsActive = false;
                    credit.CreditCurrentValue = 0;
                    credit.DateOfClose = now;
                    credit.IsEarlyRepaymentRequested = false;
                    continue;
                }
                decimal monthlyRate = credit.InterestRate / 12m / 100m;
                int n = credit.PeriodOfPayment;
                decimal S = credit.CreditStartValue;
                decimal payment = S * (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), n)) /
                    ((decimal)Math.Pow((double)(1 + monthlyRate), n) - 1);
                var account = await _context.Accounts.FirstOrDefaultAsync(a => a.RecordId == credit.RecordId);
                if (account != null)
                {
                    if (credit.CreditCurrentValue > 0)
                    {
                        decimal interestPart = credit.CreditCurrentValue * monthlyRate;
                        decimal principalPart = payment - interestPart;
                        if (account.Balance >= payment)
                        {
                            account.Balance -= payment;
                            credit.CreditCurrentValue -= principalPart;
                            if (credit.CreditCurrentValue < 0) credit.CreditCurrentValue = 0;
                        }
                        else
                        {
                            credit.OverdueCount++;
                            decimal penalty = payment * 0.05m;
                            credit.PenaltySum += penalty;
                            AddPenaltyHistory(credit, penalty, $"Просрочка платежа за {now:yyyy-MM}");
                        }
                    }
                }
                if (credit.IsEarlyRepaymentRequested && credit.CreditCurrentValue > 0 && monthsPassed > 0)
                {
                    if (account != null && account.Balance >= credit.CreditCurrentValue)
                    {
                        account.Balance -= credit.CreditCurrentValue;
                        credit.CreditCurrentValue = 0;
                        credit.IsActive = false;
                        credit.DateOfClose = now;
                        credit.IsEarlyRepaymentRequested = false;
                    }
                }
            }
            await _context.SaveChangesAsync();
        }

        public class CreditPenaltyHistoryItem
        {
            public DateTime Date { get; set; }
            public decimal PenaltyAmount { get; set; }
            public string Reason { get; set; } = string.Empty;
        }
    }
}
