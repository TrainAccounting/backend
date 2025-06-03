using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Trainacc.Data;
using Trainacc.Models;
using Trainacc.Services;
using Xunit;

namespace Trainacc.Tests
{
    public class CreditsServiceTests
    {
        private AppDbContext GetDb() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(System.Guid.NewGuid().ToString()).Options);

        [Fact]
        public async Task CreateCreditAsync_CreatesCredit()
        {
            var db = GetDb();
            var acc = new Account();
            db.Accounts.Add(acc); db.SaveChanges();
            var service = new CreditsService(db);
            var dto = new CreditDto { NameOfCredit = "Test", CreditStartValue = 1000, InterestRate = 10, PeriodOfPayment = 12 };
            var result = await service.CreateCreditAsync(dto, acc.Id);
            Assert.NotNull(result);
            Assert.Equal("Test", result.NameOfCredit);
        }

        [Fact]
        public async Task GetCreditsAsync_ReturnsCredits()
        {
            var db = GetDb();
            db.Credits.Add(new Credit { NameOfCredit = "A", CreditStartValue = 100, InterestRate = 10, PeriodOfPayment = 12 });
            db.SaveChanges();
            var service = new CreditsService(db);
            var list = await service.GetCreditsAsync();
            Assert.Single(list);
        }
    }
}
