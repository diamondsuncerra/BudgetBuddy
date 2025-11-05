using System.Data;

namespace BudgetBuddy.App;
public class Program
{
    public static void Main(string[]args)
    {
        Console.WriteLine("Welcome to Budget Buddy!\nPlease choose one of the following:");
        ConsoleHelper.PrintAllOptions();
        
        bool looping = true;
        while(looping)
        {
            
            string? input = Console.ReadLine().Replace(" ","", StringComparison.OrdinalIgnoreCase);

            if (!Enum.TryParse(input, true, out ConsoleCommands command))
            {
                Console.WriteLine("Unknown command.");
                continue;
            }


                switch (command)
                {
                    case ConsoleCommands.Import:
                        Console.WriteLine("Import command selected.");
                        break;

                    case ConsoleCommands.ListAll:
                        Console.WriteLine("Listing all records...");
                        break;

                    case ConsoleCommands.ListMonth:
                        Console.WriteLine("Listing records for a specific month...");
                        break;

                    case ConsoleCommands.ByCategory:
                        Console.WriteLine("Listing by category...");
                        break;

                    case ConsoleCommands.Over:
                        Console.WriteLine("Showing transactions over a specific amount...");
                        break;

                    case ConsoleCommands.Search:
                        Console.WriteLine("Searching transactions...");
                        break;

                    case ConsoleCommands.SetCategory:
                        Console.WriteLine("Setting category for a transaction...");
                        break;

                    case ConsoleCommands.RenameCategory:
                        Console.WriteLine("Renaming a category...");
                        break;

                    case ConsoleCommands.Remove:
                        Console.WriteLine("Removing a transaction...");
                        break;

                    case ConsoleCommands.Stats:
                        Console.WriteLine("Showing statistics...");
                        break;

                    case ConsoleCommands.Export:
                        Console.WriteLine("Exporting data...");
                        break;

                    case ConsoleCommands.Help:
                        Console.WriteLine("Showing help information...");
                        break;

                    case ConsoleCommands.Exit:
                        Console.WriteLine("Exiting application. Goodbye!");
                    looping = true;
                        break;

                    default:
                        Console.WriteLine("Command not implemented yet.");
                        break;
                }
            
        }
    }
}