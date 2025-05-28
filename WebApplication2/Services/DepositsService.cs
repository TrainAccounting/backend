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
            return await _context.Deposits.Select(d => new DepositDto
            {
                Id = d.Id,
                NameOfDeposit = d.NameOfDeposit,
                DepositStartValue = d.DepositStartValue,
                DepositCurrentValue = d.DepositCurrentValue,
                DateOfOpening = d.DateOfOpening,
                PeriodOfPayment = d.PeriodOfPayment,
                InterestRate = d.InterestRate,
                RecordId = d.RecordId
            }).ToListAsync();
        }

        public async Task<DepositDto?> GetDepositAsync(int id)
        {
            var d = await _context.Deposits.FindAsync(id);
            if (d == null) return null;
            return new DepositDto
            {
                Id = d.Id,
                NameOfDeposit = d.NameOfDeposit,
                DepositStartValue = d.DepositStartValue,
                DepositCurrentValue = d.DepositCurrentValue,
                DateOfOpening = d.DateOfOpening,
                PeriodOfPayment = d.PeriodOfPayment,
                InterestRate = d.InterestRate,
                RecordId = d.RecordId
            };
        }

        public async Task<DepositDto> CreateDepositAsync(DepositDto dto)
        {
            if (dto.InterestRate < 0)
                throw new Exception("Ставка депозита должна быть положительной");
            if (dto.DepositStartValue < 0)
                throw new Exception("Стартовая сумма депозита должна быть положительной");
            if (dto.DepositCurrentValue < 0)
                throw new Exception("Текущая сумма депозита должна быть положительной");
            if (dto.PeriodOfPayment <= 0)
                throw new Exception("Период выплат должен быть положительным");
            var deposit = new Deposit
            {
                NameOfDeposit = dto.NameOfDeposit,
                DepositStartValue = dto.DepositStartValue,
                DepositCurrentValue = dto.DepositCurrentValue,
                DateOfOpening = dto.DateOfOpening,
                PeriodOfPayment = dto.PeriodOfPayment,
                InterestRate = dto.InterestRate,
                RecordId = dto.RecordId
            };
            _context.Deposits.Add(deposit);
            await _context.SaveChangesAsync();
            dto.Id = deposit.Id;
            return dto;
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
            return await _context.Deposits
                .Where(d => d.RecordId == recordId)
                .Select(d => new DepositDto
                {
                    Id = d.Id,
                    NameOfDeposit = d.NameOfDeposit,
                    DepositStartValue = d.DepositStartValue,
                    DepositCurrentValue = d.DepositCurrentValue,
                    DateOfOpening = d.DateOfOpening,
                    PeriodOfPayment = d.PeriodOfPayment,
                    InterestRate = d.InterestRate,
                    RecordId = d.RecordId
                })
                .ToListAsync();
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
}
