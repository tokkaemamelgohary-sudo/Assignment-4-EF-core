using System.Globalization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NationalBank.Services;
using NationalBank.UI;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

var exit = false;
while (!exit)
{
    Console.Clear();
    Console.WriteLine("========================================");
    Console.WriteLine("    National Bank - Management");
    Console.WriteLine("========================================");
    Console.WriteLine("  1) Add a new Customer");
    Console.WriteLine("  2) Open a new Account for a Customer");
    Console.WriteLine("  3) Update Account Status (Active / Closed)");
    Console.WriteLine("  4) Remove an Account from a Customer");
    Console.WriteLine("  5) List all Customers (with accounts)");
    Console.WriteLine("  0) Exit");
    Console.WriteLine("----------------------------------------");
    Console.Write("  Enter choice: ");

    var choice = ConsoleHelper.ReadLine();
    Console.Clear();

    try
    {
        switch (choice)
        {
            case "1": BankOperations.AddCustomer(); break;
            case "2": BankOperations.OpenAccount(); break;
            case "3": BankOperations.UpdateAccountStatus(); break;
            case "4": BankOperations.RemoveAccount(); break;
            case "5": BankOperations.ListCustomers(); break;
            case "0": exit = true; break;
            default: ConsoleHelper.Error("Invalid choice. Please enter a number from 0 to 5."); break;
        }
    }
    catch (DbUpdateException ex)
    {
        ConsoleHelper.Error($"Could not save: {ex.GetBaseException().Message}");
    }
    catch (SqlException ex)
    {
        ConsoleHelper.Error($"Database error: {ex.Message}");
    }
    catch (InvalidOperationException ex)
    {
        ConsoleHelper.Error($"Operation failed: {ex.GetBaseException().Message}");
    }

    if (!exit) ConsoleHelper.Pause();
}

Console.WriteLine("Goodbye!");