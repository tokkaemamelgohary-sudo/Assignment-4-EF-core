using Microsoft.EntityFrameworkCore;
using NationalBank.Entities;

namespace NationalBank.Data;

public class BankDbContext : DbContext
{
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Manager> Managers => Set<Manager>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<CustomerAccount> CustomerAccounts => Set<CustomerAccount>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

   
       protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            @"Server=localhost\SQLEXPRESS;Database=NationalBankDb;Trusted_Connection=True;TrustServerCertificate=True");
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BankDbContext).Assembly);
    }
}