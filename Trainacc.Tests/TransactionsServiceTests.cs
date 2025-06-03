using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Trainacc.Data;
using Trainacc.Models;
using Trainacc.Services;
using Xunit;

namespace Trainacc.Tests
{
    public class TransactionsServiceTests
    {
        private AppDbContext GetDb() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(System.Guid.NewGuid().ToString()).Options);

        [Fact]
        public async Task CreateTransactionAsync_CreatesTransaction()
        {
            var db = GetDb();
            var acc = new Account { Balance = 1000 };
            db.Accounts.Add(acc); db.SaveChanges();
            var restrictionsService = new RestrictionsService(db);
            var service = new TransactionsService(db, restrictionsService);
            var dto = new TransactionCreateDto { Category = "Test", TransactionValue = 100, IsAdd = false };
            var result = await service.CreateTransactionAsync(dto, acc.Id);
            Assert.NotNull(result);
            Assert.Equal("Test", result.Category);
        }

        [Fact]
        public async Task GetTransactionsAsync_ReturnsTransactions()
        {
            var db = GetDb();
            db.Transactions.Add(new Transactions { Category = "A", TransactionValue = 100, IsAdd = true });
            db.SaveChanges();
            var restrictionsService = new RestrictionsService(db);
            var service = new TransactionsService(db, restrictionsService);
            var list = await service.GetTransactionsAsync();
            Assert.Single(list);
        }
    }
}
