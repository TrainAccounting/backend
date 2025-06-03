using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Trainacc.Models;
using Trainacc.Data;
using Trainacc.Services;
using Xunit;

namespace Trainacc.Tests
{
    public class RegularTransactionsServiceTests
    {
        private AppDbContext GetInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task ApplyRegularTransactionsAsync_CreatesTransaction_WhenPeriodPassed()
        {
            var db = GetInMemoryDb();
            var account = new Account { Balance = 0 };
            db.Accounts.Add(account);
            db.SaveChanges();
            var reg = new RegularTransaction
            {
                Category = "Test",
                TransactionValue = 100,
                AccountId = account.Id,
                IsAdd = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                PeriodDays = 3
            };
            db.RegularTransactions.Add(reg);
            db.SaveChanges();
            var service = new RegularTransactionsService(db);

            await service.ApplyRegularTransactionsAsync();

            var txs = await db.Transactions.ToListAsync();
            Assert.NotEmpty(txs);
            Assert.Equal(1, txs.Count);
            Assert.Equal(100, txs[0].TransactionValue);
        }

        [Fact]
        public async Task ApplyRegularTransactionsAsync_DoesNotCreateTransaction_IfPeriodNotPassed()
        {
            var db = GetInMemoryDb();
            var account = new Account { Balance = 0 };
            db.Accounts.Add(account);
            db.SaveChanges();
            var reg = new RegularTransaction
            {
                Category = "Test",
                TransactionValue = 100,
                AccountId = account.Id,
                IsAdd = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                PeriodDays = 3
            };
            db.RegularTransactions.Add(reg);
            db.SaveChanges();
            var service = new RegularTransactionsService(db);

            await service.ApplyRegularTransactionsAsync();

            var txs = await db.Transactions.ToListAsync();
            Assert.Empty(txs);
        }

        [Fact]
        public async Task ApplyRegularTransactionsAsync_CreatesMultipleTransactions_WhenMultiplePeriodsMissed()
        {
            var db = GetInMemoryDb();
            var account = new Account { Balance = 0 };
            db.Accounts.Add(account);
            db.SaveChanges();
            var reg = new RegularTransaction
            {
                Category = "Test",
                TransactionValue = 10,
                AccountId = account.Id,
                IsAdd = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                PeriodDays = 3
            };
            db.RegularTransactions.Add(reg);
            db.SaveChanges();
            var service = new RegularTransactionsService(db);
            await service.ApplyRegularTransactionsAsync();
            var txs = await db.Transactions.ToListAsync();
            Assert.True(txs.Count >= 3); // минимум 3 транзакции за 10 дней
        }

        [Fact]
        public async Task ApplyRegularTransactionsAsync_Expense_UpdatesRestrictionAndBalance()
        {
            var db = GetInMemoryDb();
            var account = new Account { Balance = 100 };
            db.Accounts.Add(account);
            db.SaveChanges();
            var restriction = new Restriction { AccountId = account.Id, Category = "Food", RestrictionValue = 100, MoneySpent = 0 };
            db.Restrictions.Add(restriction);
            db.SaveChanges();
            var reg = new RegularTransaction
            {
                Category = "Food",
                TransactionValue = 30,
                AccountId = account.Id,
                IsAdd = false,
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                PeriodDays = 2
            };
            db.RegularTransactions.Add(reg);
            db.SaveChanges();
            var service = new RegularTransactionsService(db);
            await service.ApplyRegularTransactionsAsync();
            var txs = await db.Transactions.ToListAsync();
            Assert.True(txs.Count >= 2); // минимум 2 расхода
            var updatedAccount = await db.Accounts.FindAsync(account.Id);
            Assert.Equal(100 - 30 * txs.Count, updatedAccount.Balance);
            var updatedRestriction = await db.Restrictions.FindAsync(restriction.Id);
            Assert.Equal(30 * txs.Count, updatedRestriction.MoneySpent);
        }

        [Fact]
        public async Task ApplyRegularTransactionsAsync_ThrowsIfNotEnoughMoney()
        {
            var db = GetInMemoryDb();
            var account = new Account { Balance = 10 };
            db.Accounts.Add(account);
            db.SaveChanges();
            var reg = new RegularTransaction
            {
                Category = "Test",
                TransactionValue = 100,
                AccountId = account.Id,
                IsAdd = false,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                PeriodDays = 2
            };
            db.RegularTransactions.Add(reg);
            db.SaveChanges();
            var service = new RegularTransactionsService(db);
            await Assert.ThrowsAsync<Exception>(async () => await service.ApplyRegularTransactionsAsync());
        }

        [Fact]
        public async Task ApplyRegularTransactionsAsync_DoesNotDuplicateTransactions()
        {
            var db = GetInMemoryDb();
            var account = new Account { Balance = 1000 };
            db.Accounts.Add(account);
            db.SaveChanges();
            var reg = new RegularTransaction
            {
                Category = "Test",
                TransactionValue = 100,
                AccountId = account.Id,
                IsAdd = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                PeriodDays = 2
            };
            db.RegularTransactions.Add(reg);
            db.SaveChanges();
            var service = new RegularTransactionsService(db);
            await service.ApplyRegularTransactionsAsync();
            var firstCount = (await db.Transactions.ToListAsync()).Count;
            await service.ApplyRegularTransactionsAsync();
            var secondCount = (await db.Transactions.ToListAsync()).Count;
            Assert.Equal(firstCount, secondCount); // не должно быть дублей
        }

        [Fact]
        public async Task ApplyRegularTransactionsAsync_OneTimePeriod_CreatesOnlyOneTransaction()
        {
            var db = GetInMemoryDb();
            var account = new Account { Balance = 0 };
            db.Accounts.Add(account);
            db.SaveChanges();
            var reg = new RegularTransaction
            {
                Category = "Test",
                TransactionValue = 100,
                AccountId = account.Id,
                IsAdd = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10), // прошло много дней
                PeriodDays = 1
            };
            db.RegularTransactions.Add(reg);
            db.SaveChanges();
            var service = new RegularTransactionsService(db);
            await service.ApplyRegularTransactionsAsync();
            var txs = await db.Transactions.ToListAsync();
            Assert.Single(txs); // только одна транзакция
            Assert.Equal(100, txs[0].TransactionValue);
            Assert.Equal("Test", txs[0].Category);
        }

        [Fact]
        public async Task ApplyRegularTransactionsAsync_FixedPeriod_CreatesTransactionsOnlyForPlannedDays()
        {
            var db = GetInMemoryDb();
            var account = new Account { Balance = 0 };
            db.Accounts.Add(account);
            db.SaveChanges();
            // createdAt = 20.05.2025, periodDays = 1, планируем 5 дней
            var createdAt = new DateTime(2025, 5, 20, 0, 0, 0, DateTimeKind.Utc);
            var reg = new RegularTransaction
            {
                Category = "Test",
                TransactionValue = 100,
                AccountId = account.Id,
                IsAdd = true,
                CreatedAt = createdAt,
                PeriodDays = 1
            };
            db.RegularTransactions.Add(reg);
            db.SaveChanges();
            // Симулируем, что сейчас 31.05.2025
            var service = new RegularTransactionsService(db);
            // Мокаем время через рефлексию или вручную вызываем ApplyRegularTransactionsAsync столько раз, сколько нужно
            // Здесь просто вызываем один раз, т.к. логика теперь создаёт только за первые 5 дней
            await service.ApplyRegularTransactionsAsync();
            var txs = await db.Transactions.ToListAsync();
            // Должно быть 5 транзакций: 21.05, 22.05, 23.05, 24.05, 25.05
            Assert.Equal(5, txs.Count);
            for (int i = 0; i < 5; i++)
            {
                Assert.Equal(createdAt.AddDays(i + 1), txs[i].TimeOfTransaction);
            }
        }
    }
}
