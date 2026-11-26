using System.Globalization;
using System.Text.RegularExpressions;

namespace NationalBank.UI;

public static class ConsoleHelper
{
    public static string ReadLine()
    {
        var line = Console.ReadLine();
        if (line is null) Environment.Exit(0); 
        return line.Trim();
    }

    public static string ReadText(string label, Func<string, bool> isValid, string error)
    {
        while (true)
        {
            Console.Write($"{label,-15}: ");
            var value = ReadLine();
            if (isValid(value)) return value;
            Error(error);
        }
    }

    public static string ReadRequired(string label, int maxLength) =>
        ReadText(label, v => v.Length > 0 && v.Length <= maxLength,
                 $"Required, maximum {maxLength} characters.");

    public static string ReadPattern(string label, string pattern, string error) =>
        ReadText(label, v => Regex.IsMatch(v, pattern), error);

    public static int ReadPositiveInt(string label)
    {
        while (true)
        {
            Console.Write($"{label,-15}: ");
            if (int.TryParse(ReadLine(), out var number) && number > 0) return number;
            Error("Please enter a positive number.");
        }
    }

    public static DateOnly ReadBirthDate(string label)
    {
        while (true)
        {
            Console.Write($"{label,-15}: (yyyy-MM-dd) ");
            var today = DateOnly.FromDateTime(DateTime.Today);
            if (DateOnly.TryParseExact(ReadLine(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out var date)
                && date < today && date > today.AddYears(-120))
                return date;
            Error("Enter a valid past date in the format yyyy-MM-dd.");
        }
    }

    public static TEnum ReadChoice<TEnum>(string title) where TEnum : struct, Enum
    {
        Console.WriteLine($"{title}:");
        foreach (var value in Enum.GetValues<TEnum>())
            Console.WriteLine($"    {Convert.ToInt32(value)}) {value}");

        while (true)
        {
            Console.Write("  Choice: ");
            if (int.TryParse(ReadLine(), out var number))
            {
                var choice = (TEnum)Enum.ToObject(typeof(TEnum), number);
                if (Enum.IsDefined(choice)) return choice;
            }
            Error("Invalid choice, try again.");
        }
    }

    public static void Success(string message) => Write(message, ConsoleColor.Green);
    public static void Info(string message) => Write(message, ConsoleColor.Cyan);
    public static void Error(string message) => Write(message, ConsoleColor.Red);

    private static void Write(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void Pause()
    {
        Console.WriteLine();
        Console.WriteLine("Press any key to return to the menu...");
        Console.ReadKey(intercept: true);
    }
}