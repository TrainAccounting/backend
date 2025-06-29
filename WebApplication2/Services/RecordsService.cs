using System.Collections.Generic;
using System.Threading.Tasks;
using Trainacc.Models;
using Trainacc.Data;
using Microsoft.EntityFrameworkCore;

namespace Trainacc.Services
{
    public class RecordsService
    {
        private readonly AppDbContext _context;
        public RecordsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<RecordDto>> GetRecordsAsync()
        {
            return await _context.Records.Include(r => r.User).Select(r => new RecordDto
            {
                Id = r.Id,
                NameOfRecord = r.NameOfRecord,
                DateOfCreation = r.DateOfCreation,
                User = r.User != null ? new UserDto
                {
                    Id = r.User.Id,
                    FIO = r.User.FIO,
                    Email = r.User.Email,
                    Phone = r.User.Phone
                } : null
            }).ToListAsync();
        }

        public async Task<RecordDto?> GetRecordAsync(int id)
        {
            var r = await _context.Records.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);
            if (r == null) return null;
            return new RecordDto
            {
                Id = r.Id,
                NameOfRecord = r.NameOfRecord,
                DateOfCreation = r.DateOfCreation,
                User = r.User != null ? new UserDto
                {
                    Id = r.User.Id,
                    FIO = r.User.FIO,
                    Email = r.User.Email,
                    Phone = r.User.Phone
                } : null
            };
        }

        public async Task<RecordDto> CreateRecordAsync(RecordCreateDto recordDto, int userId)
        {
            var record = new Record
            {
                NameOfRecord = recordDto.NameOfRecord,
                DateOfCreation = DateTime.UtcNow,
                UserId = userId
            };
            _context.Records.Add(record);
            await _context.SaveChangesAsync();
            return new RecordDto
            {
                Id = record.Id,
                NameOfRecord = record.NameOfRecord,
                DateOfCreation = record.DateOfCreation
            };
        }

        public async Task<List<RecordDto>> GetRecordsByUserAsync(int userId)
        {
            return await _context.Records
                .Where(r => r.UserId == userId)
                .Include(r => r.User)
                .Select(r => new RecordDto
                {
                    Id = r.Id,
                    NameOfRecord = r.NameOfRecord,
                    DateOfCreation = r.DateOfCreation,
                    User = r.User != null ? new UserDto
                    {
                        Id = r.User.Id,
                        FIO = r.User.FIO,
                        Email = r.User.Email,
                        Phone = r.User.Phone
                    } : null
                })
                .ToListAsync();
        }

        public async Task<RecordDto?> UpdateRecordAsync(int id, RecordCreateDto recordDto)
        {
            var record = await _context.Records.FirstOrDefaultAsync(r => r.Id == id);
            if (record == null) return null;
            record.NameOfRecord = recordDto.NameOfRecord;
            await _context.SaveChangesAsync();
            return new RecordDto
            {
                Id = record.Id,
                NameOfRecord = record.NameOfRecord,
                DateOfCreation = record.DateOfCreation
            };
        }

        public async Task<bool> DeleteRecordAsync(int id)
        {
            var record = await _context.Records.FirstOrDefaultAsync(r => r.Id == id);
            if (record == null) return false;
            _context.Records.Remove(record);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}