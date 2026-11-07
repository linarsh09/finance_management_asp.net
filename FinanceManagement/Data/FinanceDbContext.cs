//using FinanceManagement.Models;
//using Microsoft.EntityFrameworkCore;
//namespace FinanceManagement.Data;
//public class FinanceDbContext : DbContext
//{
//    public FinanceDbContext(DbContextOptions<FinanceDbContext> options) : base(options) { }
//    public DbSet<Account> Accounts => Set<Account>();
//    public DbSet<Transaction> Transactions => Set<Transaction>();
//    public DbSet<Category> Categories => Set<Category>();
//    public DbSet<User> Users => Set<User>();
//    protected override void OnModelCreating(ModelBuilder modelBuilder)
//    {
//        base.OnModelCreating(modelBuilder);
//        modelBuilder.Entity<Transaction>().Property(t => t.Amount).HasPrecision(18, 2);
//        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
//        modelBuilder.Entity<User>().HasIndex(u => u.Login).IsUnique();
//        modelBuilder.Entity<Account>().HasOne(a => a.User).WithMany(u => u.Accounts).HasForeignKey(a => a.UserId);
//        modelBuilder.Entity<Category>().HasOne(c => c.User).WithMany(u => u.Categories).HasForeignKey(c => c.UserId);
//    }
//}
using FinanceManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Data;

public class FinanceDbContext : DbContext
{
    public FinanceDbContext(
        DbContextOptions<FinanceDbContext> options)
        : base(options) { }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Transfer> Transfers => Set<Transfer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Login)
            .IsUnique();

        modelBuilder.Entity<Account>()
            .HasOne(a => a.User)
            .WithMany(u => u.Accounts)
            .HasForeignKey(a => a.UserId);

        modelBuilder.Entity<Category>()
            .HasOne(c => c.User)
            .WithMany(u => u.Categories)
            .HasForeignKey(c => c.UserId);

        modelBuilder.Entity<Transfer>()
            .Property(t => t.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Transfer>()
            .HasOne(t => t.FromAccount)
            .WithMany()
            .HasForeignKey(t => t.FromAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transfer>()
            .HasOne(t => t.ToAccount)
            .WithMany()
            .HasForeignKey(t => t.ToAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}