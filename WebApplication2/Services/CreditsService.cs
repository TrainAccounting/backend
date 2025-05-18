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
                Amount = c.Amount,
                PayType = c.PayType,
                IsActive = c.IsActive
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
                Amount = c.Amount,
                PayType = c.PayType,
                IsActive = c.IsActive
            };
        }

        public async Task<CreditDto> CreateCreditAsync(CreditDto dto)
        {
            var credit = new Credit
            {
                NameOfCredit = dto.NameOfCredit,
                CreditCurrentValue = dto.CreditCurrentValue,
                DateOfOpening = dto.DateOfOpening,
                PeriodOfPayment = dto.PeriodOfPayment,
                InterestRate = dto.InterestRate,
                Amount = dto.Amount,
                PayType = dto.PayType,
                IsActive = dto.IsActive,
                RecordId = dto.Id
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
    }
}
