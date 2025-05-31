using Microsoft.EntityFrameworkCore;
using Trainacc.Models;

namespace Trainacc.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            // Database.EnsureDeleted();
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

            modelBuilder.Entity<Users>()
                .Property(u => u.PasswordHash)
                .IsRequired();

            modelBuilder.Entity<Users>()
                .HasMany(u => u.Records)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Record>()
                .HasMany(r => r.Restrictions)
                .WithOne(r => r.Record)
                .HasForeignKey(r => r.RecordId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Record>()
                .HasMany(r => r.Accounts)
                .WithOne(r => r.Record)
                .HasForeignKey(a => a.RecordId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Record>()
                .HasMany(r => r.Transactions)
                .WithOne(r => r.Record)
                .HasForeignKey(t => t.RecordId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Record>()
                .HasMany(r => r.Deposits)
                .WithOne(r => r.Record)
                .HasForeignKey(d => d.RecordId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Record>()
                .HasMany(r => r.Credits)
                .WithOne(r => r.Record)
                .HasForeignKey(c => c.RecordId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Transactions>()
                .Property(t => t.Type)
                .HasConversion<string>();
        }
    }
}