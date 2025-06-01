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
            var credits = await _context.Credits.ToListAsync();
            var result = new List<CreditDto>();
            foreach (var c in credits)
            {
                decimal interestRateInMoney = c.InterestRate * c.CreditStartValue / 100m;
                decimal totalToPay = c.CreditStartValue + interestRateInMoney;
                decimal monthlyPayment = c.PeriodOfPayment > 0 ? totalToPay / c.PeriodOfPayment : 0;
                var account = await _context.Accounts.FindAsync(c.AccountId);
                bool isEnoughMoney = account != null && account.Balance >= monthlyPayment;
                result.Add(new CreditDto
                {
                    Id = c.Id,
                    NameOfCredit = c.NameOfCredit,
                    CreditStartValue = c.CreditStartValue,
                    CreditCurrentValue = c.CreditCurrentValue,
                    DateOfOpening = c.DateOfOpening,
                    PeriodOfPayment = c.PeriodOfPayment,
                    InterestRate = c.InterestRate,
                    AccountId = c.AccountId,
                    TogetAccountId = c.TogetAccountId,
                    InterestRateInMoney = interestRateInMoney,
                    IsOver = c.IsOver,
                    TotalToPay = totalToPay,
                    MonthlyPayment = monthlyPayment,
                    IsEnoughMoney = isEnoughMoney
                });
            }
            return result;
        }

        public async Task<CreditDto?> GetCreditAsync(int id)
        {
            var c = await _context.Credits.FindAsync(id);
            if (c == null) return null;
            decimal interestRateInMoney = c.InterestRate * c.CreditStartValue / 100m;
            decimal totalToPay = c.CreditStartValue + interestRateInMoney;
            decimal monthlyPayment = totalToPay / c.PeriodOfPayment;
            var account = await _context.Accounts.FindAsync(c.AccountId);
            bool isEnoughMoney = account != null && account.Balance >= monthlyPayment;
            return new CreditDto
            {
                Id = c.Id,
                NameOfCredit = c.NameOfCredit,
                CreditStartValue = c.CreditStartValue,
                CreditCurrentValue = c.CreditCurrentValue,
                DateOfOpening = c.DateOfOpening,
                PeriodOfPayment = c.PeriodOfPayment,
                InterestRate = c.InterestRate,
                AccountId = c.AccountId,
                TogetAccountId = c.TogetAccountId,
                InterestRateInMoney = interestRateInMoney,
                IsOver = c.IsOver,
                TotalToPay = totalToPay,
                MonthlyPayment = monthlyPayment,
                IsEnoughMoney = isEnoughMoney
            };
        }

        public async Task<CreditDto> CreateCreditAsync(CreditDto dto, int accountsId)
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
                AccountId = accountsId,
                IsActive = true
            };
            _context.Credits.Add(credit);
            await _context.SaveChangesAsync();
            dto.Id = credit.Id;
            return dto;
        }

        public async Task<CreditDto> CreateCreditAsync(CreditCreateDto dto, int accountsId, int togetAccountId)
        {
            if (dto.InterestRate < 0)
                throw new Exception("Ставка кредита должна быть положительной");
            if (dto.CreditStartValue <= 0)
                throw new Exception("Сумма кредита должна быть положительной");
            var paymentAccount = await _context.Accounts.FindAsync(accountsId);
            if (paymentAccount == null)
                throw new Exception("Счёт для платежей не найден");
            var togetAccount = await _context.Accounts.FindAsync(togetAccountId);
            if (togetAccount == null)
                throw new Exception("Счёт для зачисления кредита не найден");
            DateTime dateOfOpening = dto.DateOfOpening ?? DateTime.UtcNow;
            if (dateOfOpening > DateTime.UtcNow)
                throw new Exception("Дата открытия не может быть в будущем");
            if (dateOfOpening < new DateTime(2000, 1, 1))
                throw new Exception("Дата открытия слишком старая");
            var credit = new Credit
            {
                NameOfCredit = dto.NameOfCredit,
                CreditStartValue = dto.CreditStartValue,
                CreditCurrentValue = dto.CreditStartValue,
                DateOfOpening = dateOfOpening,
                PeriodOfPayment = dto.PeriodOfPayment,
                InterestRate = dto.InterestRate,
                AccountId = accountsId,
                TogetAccountId = togetAccountId,
                IsActive = true
            };
            _context.Credits.Add(credit);
            togetAccount.Balance += dto.CreditStartValue;
            await _context.SaveChangesAsync();
            decimal interestRateInMoney = credit.InterestRate * credit.CreditStartValue / 100m;
            decimal totalToPay = credit.CreditStartValue + interestRateInMoney;
            decimal monthlyPayment = credit.PeriodOfPayment > 0 ? totalToPay / credit.PeriodOfPayment : 0;
            bool isEnoughMoney = paymentAccount.Balance >= monthlyPayment;
            return new CreditDto
            {
                Id = credit.Id,
                NameOfCredit = credit.NameOfCredit,
                CreditStartValue = credit.CreditStartValue,
                CreditCurrentValue = credit.CreditCurrentValue,
                DateOfOpening = credit.DateOfOpening,
                PeriodOfPayment = credit.PeriodOfPayment,
                InterestRate = credit.InterestRate,
                AccountId = credit.AccountId,
                TogetAccountId = credit.TogetAccountId,
                InterestRateInMoney = interestRateInMoney,
                IsOver = credit.IsOver,
                TotalToPay = totalToPay,
                MonthlyPayment = monthlyPayment,
                IsEnoughMoney = isEnoughMoney
            };
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
            var credits = await _context.Credits.Where(c => c.AccountId == recordId).ToListAsync();
            var result = new List<CreditDto>();
            foreach (var c in credits)
            {
                decimal interestRateInMoney = c.InterestRate * c.CreditStartValue / 100m;
                decimal totalToPay = c.CreditStartValue + interestRateInMoney;
                decimal monthlyPayment = c.PeriodOfPayment > 0 ? totalToPay / c.PeriodOfPayment : 0;
                var account = await _context.Accounts.FindAsync(c.AccountId);
                bool isEnoughMoney = account != null && account.Balance >= monthlyPayment;
                result.Add(new CreditDto
                {
                    Id = c.Id,
                    NameOfCredit = c.NameOfCredit,
                    CreditStartValue = c.CreditStartValue,
                    CreditCurrentValue = c.CreditCurrentValue,
                    DateOfOpening = c.DateOfOpening,
                    PeriodOfPayment = c.PeriodOfPayment,
                    InterestRate = c.InterestRate,
                    AccountId = c.AccountId,
                    TogetAccountId = c.TogetAccountId,
                    InterestRateInMoney = interestRateInMoney,
                    IsOver = c.IsOver,
                    TotalToPay = totalToPay,
                    MonthlyPayment = monthlyPayment,
                    IsEnoughMoney = isEnoughMoney
                });
            }
            return result;
        }

        public async Task<List<CreditDto>> GetCreditsByAccountAsync(int accountId)
        {
            var credits = await _context.Credits.Where(c => c.AccountId == accountId).ToListAsync();
            var result = new List<CreditDto>();
            foreach (var c in credits)
            {
                decimal interestRateInMoney = c.InterestRate * c.CreditStartValue / 100m;
                decimal totalToPay = c.CreditStartValue + interestRateInMoney;
                decimal monthlyPayment = c.PeriodOfPayment > 0 ? totalToPay / c.PeriodOfPayment : 0;
                var account = await _context.Accounts.FindAsync(c.AccountId);
                bool isEnoughMoney = account != null && account.Balance >= monthlyPayment;
                result.Add(new CreditDto
                {
                    Id = c.Id,
                    NameOfCredit = c.NameOfCredit,
                    CreditStartValue = c.CreditStartValue,
                    CreditCurrentValue = c.CreditCurrentValue,
                    DateOfOpening = c.DateOfOpening,
                    PeriodOfPayment = c.PeriodOfPayment,
                    InterestRate = c.InterestRate,
                    AccountId = c.AccountId,
                    TogetAccountId = c.TogetAccountId,
                    InterestRateInMoney = interestRateInMoney,
                    IsOver = c.IsOver,
                    TotalToPay = totalToPay,
                    MonthlyPayment = monthlyPayment,
                    IsEnoughMoney = isEnoughMoney
                });
            }
            return result;
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
                var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == credit.AccountId);
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

        public async Task<bool> CloseCreditAsync(int creditId, int accountId, bool isEarly = false)
        {
            var credit = await _context.Credits.FindAsync(creditId);
            if (credit == null) return false;
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null) return false;
            var now = DateTime.UtcNow;
            int monthsPassed = ((now.Year - credit.DateOfOpening.Year) * 12) + now.Month - credit.DateOfOpening.Month;
            decimal interestRateInMoney;
            decimal totalToPay;
            if (isEarly && monthsPassed < credit.PeriodOfPayment)
            {
                interestRateInMoney = credit.InterestRate * credit.CreditStartValue / 100m * monthsPassed / credit.PeriodOfPayment;
                totalToPay = credit.CreditStartValue + interestRateInMoney;
                if (account.Balance < totalToPay) return false;
                account.Balance -= totalToPay;
                _context.Credits.Remove(credit);
            }
            else
            {
                interestRateInMoney = credit.InterestRate * credit.CreditStartValue / 100m;
                totalToPay = credit.CreditStartValue + interestRateInMoney;
                if (account.Balance < totalToPay) return false;
                account.Balance -= totalToPay;
                credit.IsOver = true;
                credit.DateOfClose = now;
            }
            _context.Set<CreditOperationHistory>().Add(new CreditOperationHistory {
                CreditId = credit.Id,
                OperationType = isEarly ? "Досрочное погашение" : "Погашение",
                Amount = totalToPay,
                Date = now,
                Description = isEarly ? $"Досрочное погашение, месяцев: {monthsPassed}" : "Полное погашение"
            });
            await _context.SaveChangesAsync();
            return true;
        }

        public class CreditPenaltyHistoryItem
        {
            public DateTime Date { get; set; }
            public decimal PenaltyAmount { get; set; }
            public string Reason { get; set; } = string.Empty;
        }
    }
}
