using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Trainacc.Data;
using Trainacc.Models;
using Trainacc.Services;
using Xunit;

namespace Trainacc.Tests
{
    public class RestrictionsServiceTests
    {
        private AppDbContext GetDb() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(System.Guid.NewGuid().ToString()).Options);

        [Fact]
        public async Task CreateRestrictionAsync_CreatesRestriction()
        {
            var db = GetDb();
            var record = new Trainacc.Models.Record { NameOfRecord = "Test", DateOfCreation = DateTime.UtcNow, UserId = 1 };
            db.Records.Add(record); db.SaveChanges();
            var acc = new Account { RecordId = record.Id };
            db.Accounts.Add(acc); db.SaveChanges();
            var service = new RestrictionsService(db);
            var dto = new RestrictionCreateDto { Category = "Test", RestrictionValue = 100 };
            var result = await service.CreateRestrictionAsync(dto, acc.Id);
            Assert.NotNull(result);
            Assert.Equal("Test", result.Category);
        }

        [Fact]
        public async Task GetRestrictionsAsync_ReturnsRestrictions()
        {
            var db = GetDb();
            db.Restrictions.Add(new Restriction { Category = "A", RestrictionValue = 100 });
            db.SaveChanges();
            var service = new RestrictionsService(db);
            var list = await service.GetRestrictionsAsync();
            Assert.Single(list);
        }
    }
}
