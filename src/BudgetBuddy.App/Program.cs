namespace BudgetBuddy.App;

using System.Threading.Tasks;
using BudgetBuddy.Domain;
using BudgetBuddy.Domain.Abstractions;
using BudgetBuddy.Infrastructure;
using BudgetBuddy.Infrastructure.Export;
using BudgetBuddy.Infrastructure.Import;
using BudgetBuddy.Infrastructure.Log;
public class Program
{
    public static async Task Main(string[] args)
    {
        // fara continue!
        // variabilele sa fie denumite mai meaningful
        // incearca sa eviti out in functii
        // returnezi pur si simplu
        // fara tupluri (value type) incearca sa returnezi reference type
        // fara return asa in mijlocul functiei, toate trebuie la inceput
        
        IRepository<Transaction, string> repository = new TransactionsRepository();
        ILogger logger = new ConsoleLogger();
        ITransactionFactory transactionFactory = new TransactionFactory();
        IImportService importerService = new ImportService(repository, transactionFactory, logger);
        IExportService exportService = new ExportService(logger);
        IBudgetService budgetService = new BudgetService(repository, importerService, exportService);
        ConsoleHelper handler = new ConsoleHelper(budgetService, logger);

        Console.WriteLine(Info.Welcome);
        handler.PrintAllOptions();

        bool looping = true;
        while (looping)
        {
            if (handler.GetCommand(out ConsoleCommands command, out string[] argText))
            {
                switch (command)
                {
                    case ConsoleCommands.Import:
                        await handler.Import(argText); // todo import nu mai face in paralel de la 
                                                        // parsarea argumentelor
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
                        logger.Warn(Warnings.CommandNotImplemented);
                        break;
                }
            }
            else
            {
                logger.Warn(Warnings.InvalidCommand);
            }
        }
    }
}