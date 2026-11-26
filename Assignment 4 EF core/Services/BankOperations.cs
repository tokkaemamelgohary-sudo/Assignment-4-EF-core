using Microsoft.EntityFrameworkCore;
using NationalBank.Data;
using NationalBank.Entities;
using static NationalBank.UI.ConsoleHelper;

namespace NationalBank.Services;

public static class BankOperations
{
    private const string AccountPattern = @"^[A-Za-z0-9-]{3,20}$";
    private const string AccountError = "3-20 characters: letters, digits or '-'.";

    public static void AddCustomer()
    {
        Console.WriteLine("--- Add New Customer ---");
        using var context = new BankDbContext();

        var fullName = ReadRequired("Full Name", 100);

        string nationalId;
        while (true)
        {
            nationalId = ReadPattern("National ID", @"^[0-9]{10,14}$", "National ID must be 10-14 digits.");
            if (!context.Customers.Any(c => c.NationalId == nationalId)) break;
            Error("A customer with this National ID already exists.");
        }

        var customer = new Customer
        {
            FullName = fullName,
            NationalId = nationalId,
            DateOfBirth = ReadBirthDate("Date of Birth"),
            Email = ReadText("Email",
                v => v.Length <= 100 && System.Text.RegularExpressions.Regex.IsMatch(v, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"),
                "Invalid email address."),
            PhoneNumber = ReadPattern("Phone", @"^[0-9]{10,15}$", "Phone must be 10-15 digits."),
            Address = ReadRequired("Address", 200),
            CustomerType = ReadChoice<CustomerType>("Customer Type")
        };

        context.Customers.Add(customer);
        context.SaveChanges();

        Console.WriteLine();
        Success($"Customer created successfully. CustomerId = {customer.Id}");
    }

    public static void OpenAccount()
    {
        Console.WriteLine("--- Open New Account ---");

        var number = ReadPattern("Account Number", AccountPattern, AccountError).ToUpperInvariant();
        var type = ReadChoice<AccountType>("Account Type");
        var branchCode = ReadRequired("Branch Code", 10).ToUpperInvariant();
        var customerId = ReadPositiveInt("Customer Id");
        var role = ReadChoice<OwnershipType>("Ownership Role");

        using var context = new BankDbContext();
        Info($"Validating branch '{branchCode}' and customer #{customerId}...");

        var branch = context.Branches.Find(branchCode);
        if (branch is null) { Error($"Branch '{branchCode}' does not exist."); return; }

        if (!context.Customers.Any(c => c.Id == customerId))
        {
            Error($"Customer #{customerId} does not exist.");
            return;
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var account = context.Accounts
                             .Include(a => a.CustomerAccounts)
                             .FirstOrDefault(a => a.AccountNumber == number);
        var isNew = account is null;

        if (account is null)
        {
            account = new Account
            {
                AccountNumber = number,
                AccountType = type,
                BranchCode = branch.Code,
                OpeningDate = today,
                CurrentBalance = 0
            };
            context.Accounts.Add(account);
        }
        else
        {
            if (role != OwnershipType.CoHolder || account.AccountType != type || account.BranchCode != branch.Code)
            {
                Error("This account number already exists. To join it, use the same type and branch with role CoHolder.");
                return;
            }
            if (account.CustomerAccounts.Any(ca => ca.CustomerId == customerId))
            {
                Error("This customer is already linked to this account.");
                return;
            }
        }

        account.CustomerAccounts.Add(new CustomerAccount
        {
            CustomerId = customerId,
            OwnershipType = role,
            OwnershipStartDate = today,
            AccountStatus = AccountStatus.Active
        });

        context.SaveChanges();

        var action = isNew ? "created and linked" : "linked";
        Success($"Account '{account.AccountNumber}' {action} to customer {customerId} as {role} owner.");
    }

    public static void UpdateAccountStatus()
    {
        Console.WriteLine("--- Update Account Status ---");

        var number = ReadPattern("Account Number", AccountPattern, AccountError);
        var customerId = ReadPositiveInt("Customer Id");

        using var context = new BankDbContext();
        var link = context.CustomerAccounts
                          .FirstOrDefault(ca => ca.AccountNumber == number && ca.CustomerId == customerId);

        if (link is null) { Error("No account with this number is linked to this customer."); return; }

        Info($"Current status: {link.AccountStatus}");
        var newStatus = ReadChoice<AccountStatus>("New Status");

        if (link.AccountStatus == newStatus) { Info($"Status is already {newStatus}."); return; }

        link.AccountStatus = newStatus;
        context.SaveChanges();
        Success($"Status updated to {newStatus}.");
    }

    public static void RemoveAccount()
    {
        Console.WriteLine("--- Remove Account From Customer ---");

        var number = ReadPattern("Account Number", AccountPattern, AccountError);
        var customerId = ReadPositiveInt("Customer Id");

        using var context = new BankDbContext();
        var account = context.Accounts
                             .Include(a => a.CustomerAccounts)
                             .FirstOrDefault(a => a.AccountNumber == number);
        var link = account?.CustomerAccounts.FirstOrDefault(ca => ca.CustomerId == customerId);

        if (account is null || link is null)
        {
            Error("No account with this number is linked to this customer.");
            return;
        }

        var isLastOwner = account.CustomerAccounts.Count == 1;
        var hasTransactions = context.Transactions.Any(t => t.AccountNumber == account.AccountNumber);

        context.CustomerAccounts.Remove(link);
        if (isLastOwner && !hasTransactions) context.Accounts.Remove(account);

        context.SaveChanges();

        Console.WriteLine();
        Success("  Ownership link deleted.");
        if (isLastOwner && !hasTransactions)
            Info($"      That was the last owner - account '{account.AccountNumber}' was also removed.");
        else if (isLastOwner)
            Info($"      Account '{account.AccountNumber}' has no owners now, but is kept for its transaction history.");
    }

    public static void ListCustomers()
    {
        Console.WriteLine("--- All Customers ---");
        Console.WriteLine();

        using var context = new BankDbContext();

        var customers = context.Customers
                               .AsNoTracking()
                               .Include(c => c.CustomerAccounts)
                                   .ThenInclude(ca => ca.Account)
                                       .ThenInclude(a => a.Branch)
                               .OrderBy(c => c.Id)
                               .ToList();

        if (customers.Count == 0) { Info("No customers found."); return; }

        foreach (var customer in customers)
        {
            Console.WriteLine($"  #{customer.Id} {customer.FullName} ({customer.CustomerType})");

            if (customer.CustomerAccounts.Count == 0)
            {
                Console.WriteLine("      (no accounts)");
                continue;
            }

            foreach (var ca in customer.CustomerAccounts.OrderBy(x => x.AccountNumber))
            {
                var a = ca.Account;
                Console.WriteLine(
                    $"      {a.AccountNumber,-10} {a.AccountType,-9} Balance: {a.CurrentBalance,12:N2}   " +
                    $"{ca.OwnershipType,-8} {ca.AccountStatus,-7} @ {a.Branch.Name}");
            }
        }
    }
}