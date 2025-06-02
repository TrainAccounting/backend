using System.Collections.Generic;
using System.Threading.Tasks;
using Trainacc.Models;
using Trainacc.Data;
using Microsoft.EntityFrameworkCore;

namespace Trainacc.Services
{
    public class DepositsService
    {
        private readonly AppDbContext _context;
        public DepositsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<DepositDto>> GetDepositsAsync()
        {
            var deposits = await _context.Deposits.ToListAsync();
            var result = new List<DepositDto>();
            var now = DateTime.UtcNow;
            foreach (var d in deposits)
            {
                int monthsPassed = ((now.Year - d.DateOfOpening.Year) * 12) + now.Month - d.DateOfOpening.Month;
                decimal interestRateInMoney = d.InterestRate * d.DepositStartValue / 100m;
                decimal currentDeposit = d.DepositStartValue + interestRateInMoney * monthsPassed;
                result.Add(new DepositDto
                {
                    Id = d.Id,
                    NameOfDeposit = d.NameOfDeposit,
                    DepositStartValue = d.DepositStartValue,
                    DepositCurrentValue = currentDeposit,
                    DateOfOpening = d.DateOfOpening,
                    PeriodOfPayment = d.PeriodOfPayment,
                    InterestRate = d.InterestRate,
                    AccountId = d.AccountId,
                    InterestRateInMoney = interestRateInMoney,
                    IsOver = d.IsOver
                });
            }
            return result;
        }

        public async Task<DepositDto?> GetDepositAsync(int id)
        {
            var d = await _context.Deposits.FindAsync(id);
            if (d == null) return null;
            var now = DateTime.UtcNow;
            int monthsPassed = ((now.Year - d.DateOfOpening.Year) * 12) + now.Month - d.DateOfOpening.Month;
            decimal interestRateInMoney = d.InterestRate * d.DepositStartValue / 100m;
            decimal currentDeposit = d.DepositStartValue + interestRateInMoney * monthsPassed;
            return new DepositDto
            {
                Id = d.Id,
                NameOfDeposit = d.NameOfDeposit,
                DepositStartValue = d.DepositStartValue,
                DepositCurrentValue = currentDeposit,
                DateOfOpening = d.DateOfOpening,
                PeriodOfPayment = d.PeriodOfPayment,
                InterestRate = d.InterestRate,
                AccountId = d.AccountId,
                InterestRateInMoney = interestRateInMoney,
                IsOver = d.IsOver
            };
        }

        public async Task<DepositDto> CreateDepositAsync(DepositCreateDto dto, int accountsId, int sourceAccountId)
        {
            if (dto.InterestRate < 0)
                throw new Exception("Ставка депозита должна быть положительной");
            if (dto.DepositStartValue < 0)
                throw new Exception("Стартовая сумма депозита должна быть положительной");
            if (dto.PeriodOfPayment <= 0)
                throw new Exception("Период выплат должен быть положительным");
            DateTime dateOfOpening = dto.DateOfOpening ?? DateTime.UtcNow;
            if (dateOfOpening > DateTime.UtcNow)
                throw new Exception("Дата открытия не может быть в будущем");
            if (dateOfOpening < new DateTime(2000, 1, 1))
                throw new Exception("Дата открытия слишком старая");
            var sourceAccount = await _context.Accounts.FindAsync(sourceAccountId);
            if (sourceAccount == null)
                throw new Exception("Счёт-источник не найден");
            if (sourceAccount.Balance < dto.DepositStartValue)
                throw new Exception("Недостаточно средств на счёте-источнике для депозита");
            var depositAccount = await _context.Accounts.FindAsync(accountsId);
            if (depositAccount == null)
                throw new Exception("Счёт для депозита не найден");
            sourceAccount.Balance -= dto.DepositStartValue;
            var deposit = new Deposit
            {
                NameOfDeposit = dto.NameOfDeposit,
                DepositStartValue = dto.DepositStartValue,
                DepositCurrentValue = dto.DepositStartValue,
                DateOfOpening = dateOfOpening,
                PeriodOfPayment = dto.PeriodOfPayment,
                InterestRate = dto.InterestRate,
                AccountId = accountsId,
                IsActive = true
            };
            _context.Deposits.Add(deposit);
            await _context.SaveChangesAsync();
            decimal interestRateInMoney = deposit.InterestRate * deposit.DepositStartValue / 100m;
            return new DepositDto
            {
                Id = deposit.Id,
                NameOfDeposit = deposit.NameOfDeposit,
                DepositStartValue = deposit.DepositStartValue,
                DepositCurrentValue = deposit.DepositCurrentValue,
                DateOfOpening = deposit.DateOfOpening,
                PeriodOfPayment = deposit.PeriodOfPayment,
                InterestRate = deposit.InterestRate,
                AccountId = deposit.AccountId,
                InterestRateInMoney = interestRateInMoney,
                IsOver = deposit.IsOver
            };
        }

        public async Task<bool> UpdateDepositAsync(int id, DepositUpdateDto dto)
        {
            var deposit = await _context.Deposits.FindAsync(id);
            if (deposit == null) return false;
            if (dto.DepositCurrentValue.HasValue && dto.DepositCurrentValue.Value < 0)
                throw new Exception("Текущая сумма депозита должна быть положительной");
            if (dto.PeriodOfPayment.HasValue && dto.PeriodOfPayment.Value <= 0)
                throw new Exception("Период выплат должен быть положительным");
            if (dto.InterestRate.HasValue && dto.InterestRate.Value < 0)
                throw new Exception("Ставка депозита должна быть положительной");
            deposit.NameOfDeposit = dto.NameOfDeposit ?? deposit.NameOfDeposit;
            deposit.DepositCurrentValue = dto.DepositCurrentValue ?? deposit.DepositCurrentValue;
            deposit.PeriodOfPayment = dto.PeriodOfPayment ?? deposit.PeriodOfPayment;
            deposit.InterestRate = dto.InterestRate ?? deposit.InterestRate;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteDepositAsync(int id)
        {
            var deposit = await _context.Deposits.FindAsync(id);
            if (deposit == null) return false;
            _context.Deposits.Remove(deposit);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<DepositDto>> GetDepositsByRecordAsync(int recordId)
        {
            var deposits = await _context.Deposits.Where(d => d.AccountId == recordId).ToListAsync();
            var result = new List<DepositDto>();
            var now = DateTime.UtcNow;
            foreach (var d in deposits)
            {
                int monthsPassed = ((now.Year - d.DateOfOpening.Year) * 12) + now.Month - d.DateOfOpening.Month;
                decimal interestRateInMoney = d.InterestRate * d.DepositStartValue / 100m;
                decimal currentDeposit = d.DepositStartValue + interestRateInMoney * monthsPassed;
                result.Add(new DepositDto
                {
                    Id = d.Id,
                    NameOfDeposit = d.NameOfDeposit,
                    DepositStartValue = d.DepositStartValue,
                    DepositCurrentValue = currentDeposit,
                    DateOfOpening = d.DateOfOpening,
                    PeriodOfPayment = d.PeriodOfPayment,
                    InterestRate = d.InterestRate,
                    AccountId = d.AccountId,
                    InterestRateInMoney = interestRateInMoney,
                    IsOver = d.IsOver
                });
            }
            return result;
        }

        public async Task<List<DepositDto>> GetDepositsByAccountAsync(int accountId)
        {
            var deposits = await _context.Deposits.Where(d => d.AccountId == accountId).ToListAsync();
            var result = new List<DepositDto>();
            var now = DateTime.UtcNow;
            foreach (var d in deposits)
            {
                int monthsPassed = ((now.Year - d.DateOfOpening.Year) * 12) + now.Month - d.DateOfOpening.Month;
                decimal interestRateInMoney = d.InterestRate * d.DepositStartValue / 100m;
                decimal currentDeposit = d.DepositStartValue + interestRateInMoney * monthsPassed;
                result.Add(new DepositDto
                {
                    Id = d.Id,
                    NameOfDeposit = d.NameOfDeposit,
                    DepositStartValue = d.DepositStartValue,
                    DepositCurrentValue = currentDeposit,
                    DateOfOpening = d.DateOfOpening,
                    PeriodOfPayment = d.PeriodOfPayment,
                    InterestRate = d.InterestRate,
                    AccountId = d.AccountId,
                    InterestRateInMoney = interestRateInMoney,
                    IsOver = d.IsOver
                });
            }
            return result;
        }

        public async Task ProcessMonthlyDepositsAsync()
        {
            var now = DateTime.UtcNow;
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
                decimal monthlyRate = deposit.InterestRate / 100m;
                decimal interest = deposit.DepositCurrentValue * monthlyRate;
                if (deposit.Capitalisation)
                {
                    deposit.DepositCurrentValue += interest;
                }
                else
                {
                    var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == deposit.AccountId);
                    if (account != null)
                        account.Balance += interest;
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CloseDepositAsync(int depositId, int accountId, bool isEarly = false)
        {
            var deposit = await _context.Deposits.FindAsync(depositId);
            if (deposit == null) return false;
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null) return false;
            var now = DateTime.UtcNow;
            int monthsPassed = ((now.Year - deposit.DateOfOpening.Year) * 12) + now.Month - deposit.DateOfOpening.Month;
            decimal interestRateInMoney;
            decimal payout;
            if (isEarly && monthsPassed < deposit.PeriodOfPayment)
            {
                interestRateInMoney = deposit.InterestRate * deposit.DepositStartValue / 100m * monthsPassed / deposit.PeriodOfPayment / 3m;
                payout = deposit.DepositStartValue + interestRateInMoney;
                account.Balance += payout;
                _context.Deposits.Remove(deposit);
            }
            else
            {
                interestRateInMoney = deposit.InterestRate * deposit.DepositStartValue / 100m * monthsPassed / deposit.PeriodOfPayment;
                payout = deposit.DepositStartValue + interestRateInMoney;
                account.Balance += payout;
                deposit.IsOver = true;
                deposit.DateOfClose = now;
            }
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
