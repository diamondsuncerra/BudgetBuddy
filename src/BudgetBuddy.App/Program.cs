namespace BudgetBuddy.App;
using System.Threading.Tasks;
using BudgetBuddy.Domain;
using BudgetBuddy.Domain.Abstractions;
using BudgetBuddy.Infrastructure;
using BudgetBuddy.Infrastructure.Import;
using BudgetBuddy.Infrastructure.Log;
public class Program
{
    public static async Task Main(string[] args)
    {
        IRepository<Transaction, string> repository = new TransactionsRepository();
        ILogger consoleLogger = new ConsoleLogger();
        ILogger fileLogger = new FileLogger();
        IImporter importer = new CsvImportAdapter(repository, fileLogger);
        ConsoleHelper handler = new ConsoleHelper(repository, consoleLogger, importer);

        Console.WriteLine(Info.Welcome);
        handler.PrintAllOptions();

        bool looping = true;
        while (looping)
        {

            if (!handler.GetCommand(out ConsoleCommands command, out string[] argText))
            {
                consoleLogger.Warn(Warnings.InvalidCommand);
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
                    consoleLogger.Warn(Warnings.CommandNotImplemented);
                    break;
            }

        }
    }
}