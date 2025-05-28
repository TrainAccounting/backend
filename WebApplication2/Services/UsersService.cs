using Microsoft.EntityFrameworkCore;
using Trainacc.Data;
using Trainacc.Models;

namespace Trainacc.Services
{
    public class UsersService
    {
        private readonly AppDbContext _context;
        public UsersService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserDto>> GetUsersAsync()
        {
            return await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FIO = u.FIO,
                    Email = u.Email,
                    Phone = u.Phone,
                    Role = u.Role
                })
                .ToListAsync();
        }

        public async Task<UserDto?> GetUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;
            return new UserDto
            {
                Id = user.Id,
                FIO = user.FIO,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role
            };
        }

        public async Task<bool> UpdateUserAsync(int id, UserUpdateDto userDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;
            if (!string.IsNullOrWhiteSpace(userDto.FIO)) user.FIO = userDto.FIO;
            if (!string.IsNullOrWhiteSpace(userDto.Email)) user.Email = userDto.Email;
            if (!string.IsNullOrWhiteSpace(userDto.Phone)) user.Phone = userDto.Phone;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(Users? user, Record? record, Account? account, string? error)> RegisterUserWithRecordAndAccountAsync(UserCreateDto userDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
                return (null, null, null, "Пользователь с таким email уже существует.");

            if (!string.IsNullOrWhiteSpace(userDto.Phone))
            {
                if (await _context.Users.AnyAsync(u => u.Phone == userDto.Phone))
                    return (null, null, null, "Пользователь с таким номером телефона уже существует.");
            }

            var user = new Users
            {
                FIO = userDto.FIO,
                Email = userDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
                Role = userDto.Role ?? string.Empty
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var record = new Record
            {
                NameOfRecord = "Default Record",
                DateOfCreation = DateTime.UtcNow,
                UserId = user.Id
            };
            _context.Records.Add(record);
            await _context.SaveChangesAsync();

            var account = new Account
            {
                NameOfAccount = "Default Account",
                Balance = 0,
                DateOfOpening = DateTime.UtcNow,
                RecordId = record.Id
            };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return (user, record, account, null);
        }
    }
}
