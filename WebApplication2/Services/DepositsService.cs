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
                Capitalisation = d.Capitalisation,
                Amount = d.Amount,
                PayType = d.PayType,
                IsActive = d.IsActive
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
                Capitalisation = d.Capitalisation,
                Amount = d.Amount,
                PayType = d.PayType,
                IsActive = d.IsActive
            };
        }

        public async Task<DepositDto> CreateDepositAsync(DepositDto dto)
        {
            var deposit = new Deposit
            {
                NameOfDeposit = dto.NameOfDeposit,
                DepositStartValue = dto.DepositStartValue,
                DepositCurrentValue = dto.DepositCurrentValue,
                DateOfOpening = dto.DateOfOpening,
                PeriodOfPayment = dto.PeriodOfPayment,
                InterestRate = dto.InterestRate,
                Capitalisation = dto.Capitalisation,
                Amount = dto.Amount,
                PayType = dto.PayType,
                IsActive = dto.IsActive,
                RecordId = dto.Id
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
            deposit.NameOfDeposit = dto.NameOfDeposit ?? deposit.NameOfDeposit;
            deposit.DepositCurrentValue = dto.DepositCurrentValue ?? deposit.DepositCurrentValue;
            deposit.PeriodOfPayment = dto.PeriodOfPayment ?? deposit.PeriodOfPayment;
            deposit.InterestRate = dto.InterestRate ?? deposit.InterestRate;
            deposit.Capitalisation = dto.Capitalisation ?? deposit.Capitalisation;
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
                    Capitalisation = d.Capitalisation,
                    Amount = d.Amount,
                    PayType = d.PayType,
                    IsActive = d.IsActive
                })
                .ToListAsync();
        }
    }
}
