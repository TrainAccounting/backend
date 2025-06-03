using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Trainacc.Data;
using Trainacc.Models;
using Trainacc.Services;
using Xunit;

namespace Trainacc.Tests
{
    public class DepositsServiceTests
    {
        private AppDbContext GetDb() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(System.Guid.NewGuid().ToString()).Options);

        [Fact]
        public async Task CreateDepositAsync_CreatesDeposit()
        {
            var db = GetDb();
            var acc = new Account { Balance = 2000 };
            db.Accounts.Add(acc); db.SaveChanges();
            var service = new DepositsService(db);
            var dto = new DepositCreateDto { NameOfDeposit = "Test", DepositStartValue = 1000, InterestRate = 10, PeriodOfPayment = 12 };
            var result = await service.CreateDepositAsync(dto, acc.Id, acc.Id);
            Assert.NotNull(result);
            Assert.Equal("Test", result.NameOfDeposit);
        }

        [Fact]
        public async Task GetDepositsAsync_ReturnsDeposits()
        {
            var db = GetDb();
            db.Deposits.Add(new Deposit { NameOfDeposit = "A", DepositStartValue = 100, InterestRate = 10, PeriodOfPayment = 12 });
            db.SaveChanges();
            var service = new DepositsService(db);
            var list = await service.GetDepositsAsync();
            Assert.Single(list);
        }
    }
}
