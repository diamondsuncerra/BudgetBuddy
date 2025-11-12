namespace BudgetBuddy.App;

using System.Data;
using System.Threading.Tasks;
using BudgetBuddy.Domain;
using BudgetBuddy.Infrastructure;
using BudgetBuddy.Infrastructure.Import;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Welcome to Budget Buddy!\nPlease choose one of the following:");
        ConsoleHelper.PrintAllOptions();
        IRepository<Transaction, string> repo = new TransactionsRepository();
        CSVImporter importer = new(repo);


        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true; 
            cts.Cancel();
            Console.WriteLine("Cancelling... please wait.");
        };

        bool looping = true;
        while (looping)
        {

            if(!ConsoleHelper.GetCommand(out ConsoleCommands command, out string [] argText))
            {
                Logger.Warn(Warnings.INVALID_COMMAND);
            }

            switch (command)
            {
                case ConsoleCommands.Import: await ConsoleHelper.Import(argText, repo, cts.Token);  break;

                case ConsoleCommands.List: ConsoleHelper.List(argText, repo); break;

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
                    ConsoleHelper.PrintAllOptions();
                    break;

                case ConsoleCommands.Exit:
                    Console.WriteLine("Exiting application. Goodbye!");
                    looping = false;
                    break;

                default:
                    Console.WriteLine("Command not implemented yet.");
                    break;
            }

        }
    }
}