using System.Collections.Generic;
using System.Threading.Tasks;
using Trainacc.Models;
using Trainacc.Data;
using Microsoft.EntityFrameworkCore;

namespace Trainacc.Services
{
    public class TransactionsService
    {
        private readonly AppDbContext _context;
        public TransactionsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TransactionDto>> GetTransactionsAsync()
        {
            return await _context.Transactions.Select(t => new TransactionDto
            {
                Id = t.Id,
                Category = t.Category,
                TransactionValue = t.TransactionValue,
                TimeOfTransaction = t.TimeOfTransaction
            }).ToListAsync();
        }

        public async Task<TransactionDto?> GetTransactionAsync(int id)
        {
            var t = await _context.Transactions.FindAsync(id);
            if (t == null) return null;
            return new TransactionDto
            {
                Id = t.Id,
                Category = t.Category,
                TransactionValue = t.TransactionValue,
                TimeOfTransaction = t.TimeOfTransaction
            };
        }

        public async Task<TransactionDto> CreateTransactionAsync(TransactionCreateDto dto)
        {
            var transaction = new Transactions
            {
                Category = dto.Category,
                TransactionValue = dto.TransactionValue,
                TimeOfTransaction = DateTime.UtcNow,
                RecordId = dto.RecordId
            };
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return new TransactionDto
            {
                Id = transaction.Id,
                Category = transaction.Category,
                TransactionValue = transaction.TransactionValue,
                TimeOfTransaction = transaction.TimeOfTransaction
            };
        }

        public async Task<bool> UpdateTransactionAsync(int id, TransactionUpdateDto dto)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return false;
            transaction.Category = dto.Category ?? transaction.Category;
            transaction.TransactionValue = dto.TransactionValue ?? transaction.TransactionValue;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTransactionAsync(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return false;
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
