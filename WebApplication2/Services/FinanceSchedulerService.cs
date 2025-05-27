using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Trainacc.Data;
using Trainacc.Models;
using System.Text.Json;
using System.Collections.Generic;

namespace Trainacc.Services
{
    public class FinanceSchedulerService
    {
        private readonly AppDbContext _context;
        public FinanceSchedulerService(AppDbContext context)
        {
            _context = context;
        }

        private void AddPenaltyHistory(Credit credit, decimal penalty, string reason)
        {
            var history = new List<CreditPenaltyHistoryItem>();
            if (!string.IsNullOrEmpty(credit.PenaltyHistoryJson))
            {
                try { history = JsonSerializer.Deserialize<List<CreditPenaltyHistoryItem>>(credit.PenaltyHistoryJson) ?? new(); }
                catch { history = new List<CreditPenaltyHistoryItem>(); }
            }
            history.Add(new CreditPenaltyHistoryItem
            {
                Date = DateTime.UtcNow,
                PenaltyAmount = penalty,
                Reason = reason
            });
            credit.PenaltyHistoryJson = JsonSerializer.Serialize(history);
        }

        public async Task ProcessMonthlyFinanceAsync()
        {
            var now = DateTime.UtcNow;
            var credits = await _context.Credits.Where(c => c.IsActive).ToListAsync();
            foreach (var credit in credits)
            {
                if (credit.PeriodOfPayment <= 0 || credit.Amount <= 0) continue;
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
                decimal S = credit.Amount;
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

            var deposits = await _context.Deposits.Where(d => d.IsActive).ToListAsync();
            foreach (var deposit in deposits)
            {
                if (deposit.PeriodOfPayment <= 0 || deposit.DepositCurrentValue <= 0) continue;
                int monthsPassed = ((now.Year - deposit.DateOfOpening.Year) * 12) + now.Month - deposit.DateOfOpening.Month;
                if (monthsPassed >= deposit.PeriodOfPayment)
                {
                    deposit.IsActive = false;
                    deposit.DateOfClose = now;
                    continue;
                }
                decimal monthlyRate = deposit.InterestRate / 12m / 100m;
                decimal interest = deposit.DepositCurrentValue * monthlyRate;
                if (deposit.Capitalisation)
                {
                    deposit.DepositCurrentValue += interest;
                }
                else
                {
                    var account = await _context.Accounts.FirstOrDefaultAsync(a => a.RecordId == deposit.RecordId);
                    if (account != null)
                        account.Balance += interest;
                }
            }
            await _context.SaveChangesAsync();
        }
    }

    public class CreditPenaltyHistoryItem
    {
        public DateTime Date { get; set; }
        public decimal PenaltyAmount { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
