using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Trainacc.Data;
using Trainacc.Models;
using Trainacc.Services;
using Xunit;

namespace Trainacc.Tests
{
    public class UsersServiceTests
    {
        private AppDbContext GetDb() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(System.Guid.NewGuid().ToString()).Options);

        [Fact]
        public async Task GetUsersAsync_ReturnsUsers()
        {
            var db = GetDb();
            db.Users.Add(new Users { FIO = "Test", Email = "a@a.a", PasswordHash = "hash", Role = "User" });
            db.SaveChanges();
            var service = new UsersService(db);
            var list = await service.GetUsersAsync();
            Assert.Single(list);
        }
    }
}
