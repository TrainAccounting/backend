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
                CreditCurrentValue = c.CreditCurrentValue,
                DateOfOpening = c.DateOfOpening,
                PeriodOfPayment = c.PeriodOfPayment,
                InterestRate = c.InterestRate,
                Amount = c.Amount
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
                CreditCurrentValue = c.CreditCurrentValue,
                DateOfOpening = c.DateOfOpening,
                PeriodOfPayment = c.PeriodOfPayment,
                InterestRate = c.InterestRate,
                Amount = c.Amount
            };
        }

        public async Task<CreditDto> CreateCreditAsync(CreditDto dto)
        {
            if (dto.InterestRate < 0 || dto.Amount < 0)
                throw new Exception("Ставка и сумма кредита должны быть положительными");
            var credit = new Credit
            {
                NameOfCredit = dto.NameOfCredit,
                CreditCurrentValue = dto.CreditCurrentValue,
                DateOfOpening = dto.DateOfOpening,
                PeriodOfPayment = dto.PeriodOfPayment,
                InterestRate = dto.InterestRate,
                Amount = dto.Amount,
                RecordId = dto.RecordId
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
                    CreditCurrentValue = c.CreditCurrentValue,
                    DateOfOpening = c.DateOfOpening,
                    PeriodOfPayment = c.PeriodOfPayment,
                    InterestRate = c.InterestRate,
                    Amount = c.Amount
                })
                .ToListAsync();
        }
    }
}
