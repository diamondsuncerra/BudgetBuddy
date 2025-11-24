namespace BudgetBuddy.App;

using System.Data;
using System.Threading.Tasks;
using BudgetBuddy.Domain;
using BudgetBuddy.Infrastructure;
using BudgetBuddy.Infrastructure.Import;
using BudgetBuddy.Infrastructure.Log;

public class Program
{
    public static async Task Main(string[] args)
    {
        IRepository<Transaction, string> repository = new TransactionsRepository();
        ILogger consoleLoger = new ConsoleLogger();
        ILogger fileLogger = new FileLogger();
        CSVImporter importer = new(repository, fileLogger);
        ConsoleHelper handler = new ConsoleHelper(repository, consoleLoger, importer);

        Console.WriteLine(Info.Welcome);
        handler.PrintAllOptions();

        bool looping = true;
        while (looping)
        {

            if (!handler.GetCommand(out ConsoleCommands command, out string[] argText))
            {
                consoleLoger.Warn(Warnings.InvalidCommand);
                continue;
            }

            switch (command)
            {
                case ConsoleCommands.Import:
                    await handler.Import(argText);
                    break;

                case ConsoleCommands.ListAll:
                    handler.ListAll();
                    break;
                case ConsoleCommands.ListMonth:
                    handler.ListMonth(argText);
                    break;
                case ConsoleCommands.ByCategory:
                    handler.ByCategory(argText);
                    break;

                case ConsoleCommands.Over:
                    handler.Over(argText);
                    break;

                case ConsoleCommands.Search:
                    handler.Search(argText);
                    break;

                case ConsoleCommands.SetCategory:
                    handler.SetCategory(argText);
                    break;

                case ConsoleCommands.RenameCategory:
                    handler.RenameCategory(argText);
                    break;

                case ConsoleCommands.Remove:
                    handler.Remove(argText);
                    break;

                case ConsoleCommands.StatsMonth:
                    handler.StatsMonth(argText);
                    break;
                case ConsoleCommands.StatsYearly:
                    handler.StatsYearly(argText);
                    break;

                case ConsoleCommands.Export:
                    await handler.Export(argText);
                    break;

                case ConsoleCommands.Help:
                    handler.PrintAllOptions();
                    break;

                case ConsoleCommands.Exit:
                    Console.WriteLine(Info.ExitMessage);
                    looping = false;
                    break;

                default:
                    consoleLoger.Warn(Warnings.CommandNotImplemented);
                    break;
            }

        }
    }
}