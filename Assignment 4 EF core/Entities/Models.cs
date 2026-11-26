namespace NationalBank.Entities;

public class Branch
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;

    public Manager? Manager { get; set; }
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}

public class Manager
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public DateOnly HireDate { get; set; }

    public string BranchCode { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
}

public class Customer
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string NationalId { get; set; } = null!;
    public DateOnly DateOfBirth { get; set; }
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Address { get; set; } = null!;
    public CustomerType CustomerType { get; set; }

    public ICollection<CustomerAccount> CustomerAccounts { get; set; } = new List<CustomerAccount>();
}

public class Account
{
    public string AccountNumber { get; set; } = null!;
    public decimal CurrentBalance { get; set; }
    public AccountType AccountType { get; set; }
    public DateOnly OpeningDate { get; set; }

    public string BranchCode { get; set; } = null!;
    public Branch Branch { get; set; } = null!;

    public ICollection<CustomerAccount> CustomerAccounts { get; set; } = new List<CustomerAccount>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

public class CustomerAccount
{
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public string AccountNumber { get; set; } = null!;
    public Account Account { get; set; } = null!;

    public OwnershipType OwnershipType { get; set; }
    public DateOnly OwnershipStartDate { get; set; }
    public AccountStatus AccountStatus { get; set; }
}

public class Transaction
{
    public int TransactionNumber { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public TransactionType TransactionType { get; set; }
    public string? Note { get; set; }

    public string AccountNumber { get; set; } = null!;
    public Account Account { get; set; } = null!;
}