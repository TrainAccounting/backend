using Microsoft.EntityFrameworkCore;
using Trainacc.Models;

namespace Trainacc.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        public DbSet<Users> Users { get; set; }
        public DbSet<Record> Records { get; set; }
        public DbSet<Restriction> Restrictions { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transactions> Transactions { get; set; }
        public DbSet<Deposit> Deposits { get; set; }
        public DbSet<Credit> Credits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Users>().Property(u => u.PasswordHash).IsRequired();
            modelBuilder.Entity<Users>().HasMany(u => u.Records).WithOne(r => r.User).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Record>().HasMany(r => r.Restrictions).WithOne().HasForeignKey(r => r.RecordId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Record>().HasMany(r => r.Accounts).WithOne().HasForeignKey(a => a.RecordId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Record>().HasMany(r => r.Transactions).WithOne().HasForeignKey(t => t.RecordId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Record>().HasMany(r => r.Deposits).WithOne().HasForeignKey(d => d.RecordId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Record>().HasMany(r => r.Credits).WithOne().HasForeignKey(c => c.RecordId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}