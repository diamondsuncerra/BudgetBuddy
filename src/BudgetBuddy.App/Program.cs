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

            if (!ConsoleHelper.GetCommand(out ConsoleCommands command, out string[] argText))
            {
                Logger.Warn(Warnings.INVALID_COMMAND);
                continue;
            }

            switch (command)
            {
                case ConsoleCommands.Import: await ConsoleHelper.Import(argText, repo, cts.Token); break;

                case ConsoleCommands.List: ConsoleHelper.List(argText, repo); break;

                case ConsoleCommands.ByCategory: ConsoleHelper.ByCategory(argText, repo);  break;

                case ConsoleCommands.Over: ConsoleHelper.Over(argText, repo); break;

                case ConsoleCommands.Search: ConsoleHelper.Search(argText, repo); break;

                case ConsoleCommands.SetCategory: ConsoleHelper.SetCategory(argText, repo);  break;

                case ConsoleCommands.RenameCategory: ConsoleHelper.RenameCategory(argText, repo);  break;

                case ConsoleCommands.Remove: ConsoleHelper.Remove(argText, repo);  break;

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