using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Trainacc.Data;
using Trainacc.Models;
using Trainacc.Services;
using Xunit;

namespace Trainacc.Tests
{
    public class AccountsServiceTests
    {
        private AppDbContext GetDb() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(System.Guid.NewGuid().ToString()).Options);

        [Fact]
        public async Task CreateAccountAsync_CreatesAccount()
        {
            var db = GetDb();
            var record = new Trainacc.Models.Record();
            db.Records.Add(record); db.SaveChanges();
            var service = new AccountsService(db);
            var dto = new AccountCreateDto { NameOfAccount = "Test" };
            var result = await service.CreateAccountAsync(dto, record.Id);
            Assert.NotNull(result);
            Assert.Equal("Test", result.NameOfAccount);
        }

        [Fact]
        public async Task GetAccountsAsync_ReturnsAccounts()
        {
            var db = GetDb();
            db.Accounts.Add(new Account { NameOfAccount = "A" });
            db.SaveChanges();
            var service = new AccountsService(db);
            var list = await service.GetAccountsAsync();
            Assert.Single(list);
        }

        [Fact]
        public async Task UpdateAccountAsync_UpdatesName()
        {
            var db = GetDb();
            var acc = new Account { NameOfAccount = "Old" };
            db.Accounts.Add(acc); db.SaveChanges();
            var service = new AccountsService(db);
            var ok = await service.UpdateAccountAsync(acc.Id, new AccountUpdateDto { NameOfAccount = "New" });
            Assert.True(ok);
            Assert.Equal("New", db.Accounts.Find(acc.Id)?.NameOfAccount);
        }

        [Fact]
        public async Task DeleteAccountAsync_DeletesAccount()
        {
            var db = GetDb();
            var acc = new Account { NameOfAccount = "Del" };
            db.Accounts.Add(acc); db.SaveChanges();
            var service = new AccountsService(db);
            var ok = await service.DeleteAccountAsync(acc.Id);
            Assert.True(ok);
            Assert.Null(db.Accounts.Find(acc.Id));
        }
    }
}
