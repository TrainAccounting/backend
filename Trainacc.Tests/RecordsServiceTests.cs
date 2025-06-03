using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Trainacc.Data;
using Trainacc.Models;
using Trainacc.Services;
using Xunit;

namespace Trainacc.Tests
{
    public class RecordsServiceTests
    {
        private AppDbContext GetDb() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(System.Guid.NewGuid().ToString()).Options);

        [Fact]
        public async Task CreateRecordAsync_CreatesRecord()
        {
            var db = GetDb();
            var user = new Users { FIO = "Test", Email = "a@a.a", PasswordHash = "hash", Role = "User" };
            db.Users.Add(user); db.SaveChanges();
            var service = new RecordsService(db);
            var dto = new RecordCreateDto { NameOfRecord = "Test" };
            var result = await service.CreateRecordAsync(dto, user.Id);
            Assert.NotNull(result);
            Assert.Equal("Test", result.NameOfRecord);
        }

        [Fact]
        public async Task GetRecordsAsync_ReturnsRecords()
        {
            var db = GetDb();
            var user = new Users { FIO = "Test", Email = "a@a.a", PasswordHash = "hash", Role = "User" };
            db.Users.Add(user); db.SaveChanges();
            db.Records.Add(new Trainacc.Models.Record { NameOfRecord = "A", UserId = user.Id });
            db.SaveChanges();
            var service = new RecordsService(db);
            var list = await service.GetRecordsAsync();
            Assert.Single(list);
        }
    }
}
