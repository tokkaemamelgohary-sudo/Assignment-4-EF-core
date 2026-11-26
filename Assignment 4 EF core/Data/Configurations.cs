using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NationalBank.Entities;

namespace NationalBank.Data;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.HasKey(b => b.Code);
        builder.Property(b => b.Code).HasMaxLength(10);
        builder.Property(b => b.Name).HasMaxLength(100);
        builder.Property(b => b.Address).HasMaxLength(200);
        builder.Property(b => b.PhoneNumber).HasMaxLength(15);

        builder.HasData(
            new Branch { Code = "CAI-01", Name = "Cairo Main Branch", Address = "10 Tahrir Square, Cairo", PhoneNumber = "0225750000" },
            new Branch { Code = "ALX-01", Name = "Alexandria Branch", Address = "25 Corniche Road, Alexandria", PhoneNumber = "0334870000" });
    }
}

public class ManagerConfiguration : IEntityTypeConfiguration<Manager>
{
    public void Configure(EntityTypeBuilder<Manager> builder)
    {
        builder.Property(m => m.FullName).HasMaxLength(100);
        builder.Property(m => m.Email).HasMaxLength(100);
        builder.Property(m => m.PhoneNumber).HasMaxLength(15);
        builder.HasIndex(m => m.Email).IsUnique();

        builder.HasOne(m => m.Branch)
               .WithOne(b => b.Manager)
               .HasForeignKey<Manager>(m => m.BranchCode)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new Manager { Id = 1, FullName = "Omar Hassan", Email = "omar.hassan@nationalbank.com", PhoneNumber = "01001234567", HireDate = new DateOnly(2018, 3, 1), BranchCode = "CAI-01" },
            new Manager { Id = 2, FullName = "Sara Mahmoud", Email = "sara.mahmoud@nationalbank.com", PhoneNumber = "01207654321", HireDate = new DateOnly(2020, 7, 15), BranchCode = "ALX-01" });
    }
}

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.Property(c => c.FullName).HasMaxLength(100);
        builder.Property(c => c.NationalId).HasMaxLength(14);
        builder.Property(c => c.Email).HasMaxLength(100);
        builder.Property(c => c.PhoneNumber).HasMaxLength(15);
        builder.Property(c => c.Address).HasMaxLength(200);
        builder.Property(c => c.CustomerType).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(c => c.NationalId).IsUnique();

        builder.HasData(
            new Customer { Id = 1, FullName = "Ahmed Ali", NationalId = "29001011234567", DateOfBirth = new DateOnly(1990, 1, 1), Email = "ahmed@gmail.com", PhoneNumber = "01011111111", Address = "5-Nile-Cairo-Egypt", CustomerType = CustomerType.Individual },
            new Customer { Id = 2, FullName = "Nile Trading LLC", NationalId = "30000000000002", DateOfBirth = new DateOnly(2005, 5, 20), Email = "info@niletrading.com", PhoneNumber = "01022222222", Address = "12-Port-Alexandria-Egypt", CustomerType = CustomerType.Business });
    }
}

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(a => a.AccountNumber);
        builder.Property(a => a.AccountNumber).HasMaxLength(20);
        builder.Property(a => a.CurrentBalance).HasPrecision(18, 2);
        builder.Property(a => a.AccountType).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(a => a.Branch)
               .WithMany(b => b.Accounts)
               .HasForeignKey(a => a.BranchCode)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new Account { AccountNumber = "2001-CUR", CurrentBalance = 15400m, AccountType = AccountType.Current, OpeningDate = new DateOnly(2024, 1, 10), BranchCode = "CAI-01" },
            new Account { AccountNumber = "3000-BUS", CurrentBalance = 120000m, AccountType = AccountType.Business, OpeningDate = new DateOnly(2024, 2, 5), BranchCode = "ALX-01" });
    }
}

public class CustomerAccountConfiguration : IEntityTypeConfiguration<CustomerAccount>
{
    public void Configure(EntityTypeBuilder<CustomerAccount> builder)
    {
        builder.HasKey(ca => new { ca.CustomerId, ca.AccountNumber });
        builder.Property(ca => ca.OwnershipType).HasConversion<string>().HasMaxLength(20);
        builder.Property(ca => ca.AccountStatus).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(ca => ca.Customer)
               .WithMany(c => c.CustomerAccounts)
               .HasForeignKey(ca => ca.CustomerId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ca => ca.Account)
               .WithMany(a => a.CustomerAccounts)
               .HasForeignKey(ca => ca.AccountNumber)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(
            new CustomerAccount { CustomerId = 1, AccountNumber = "2001-CUR", OwnershipType = OwnershipType.Primary, OwnershipStartDate = new DateOnly(2024, 1, 10), AccountStatus = AccountStatus.Active },
            new CustomerAccount { CustomerId = 2, AccountNumber = "3000-BUS", OwnershipType = OwnershipType.Primary, OwnershipStartDate = new DateOnly(2024, 2, 5), AccountStatus = AccountStatus.Active });
    }
}

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.TransactionNumber);
        builder.Property(t => t.Amount).HasPrecision(18, 2);
        builder.Property(t => t.TransactionType).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Note).HasMaxLength(250);
        builder.ToTable(t => t.HasCheckConstraint("CK_Transaction_Amount", "[Amount] > 0"));

        builder.HasOne(t => t.Account)
               .WithMany(a => a.Transactions)
               .HasForeignKey(t => t.AccountNumber)
               .OnDelete(DeleteBehavior.Restrict);
    }
}