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
        Console.WriteLine(Info.Welcome);
        ConsoleHelper.PrintAllOptions();
        IRepository<Transaction, string> repo = new TransactionsRepository();
        CSVImporter importer = new(repo);


        bool looping = true;
        while (looping)
        {

            if (!ConsoleHelper.GetCommand(out ConsoleCommands command, out string[] argText))
            {
                Logger.Warn(Warnings.InvalidCommand);
                continue;
            }

            switch (command)
            {
                case ConsoleCommands.Import:
                    await ConsoleHelper.Import(argText, repo);
                    break;

                case ConsoleCommands.ListAll:
                    ConsoleHelper.ListAll(repo);
                    break;
                case ConsoleCommands.ListMonth:
                    ConsoleHelper.ListMonth(argText, repo);
                    break;
                case ConsoleCommands.ByCategory:
                    ConsoleHelper.ByCategory(argText, repo);
                    break;

                case ConsoleCommands.Over:
                    ConsoleHelper.Over(argText, repo);
                    break;

                case ConsoleCommands.Search:
                    ConsoleHelper.Search(argText, repo);
                    break;

                case ConsoleCommands.SetCategory:
                    ConsoleHelper.SetCategory(argText, repo);
                    break;

                case ConsoleCommands.RenameCategory:
                    ConsoleHelper.RenameCategory(argText, repo);
                    break;

                case ConsoleCommands.Remove:
                    ConsoleHelper.Remove(argText, repo);
                    break;

                case ConsoleCommands.StatsMonth:
                    ConsoleHelper.StatsMonth(argText, repo);
                    break;
                case ConsoleCommands.StatsYearly:
                    ConsoleHelper.StatsYearly(argText, repo);
                    break;

                case ConsoleCommands.Export:
                    await ConsoleHelper.Export(argText, repo);
                    break;

                case ConsoleCommands.Help:
                    ConsoleHelper.PrintAllOptions();
                    break;

                case ConsoleCommands.Exit:
                    Console.WriteLine(Info.ExitMessage);
                    looping = false;
                    break;

                default:
                    Logger.Warn(Warnings.CommandNotImplemented);
                    break;
            }

        }
    }
}