using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BudgetBuddy.Domain;
using BudgetBuddy.Infrastructure.Import;
using Microsoft.VisualBasic;

namespace BudgetBuddy.App
{
    public class ConsoleHelper
    {
        public static void PrintAllOptions()
        {
            Console.WriteLine("Import");
            Console.WriteLine("List all");
            Console.WriteLine("List month <yyyy-MM>");
            Console.WriteLine("By category <name>");
            Console.WriteLine("Over <amount>");
            Console.WriteLine("Search <text>");
            Console.WriteLine("Set category <id> <name>");
            Console.WriteLine("Rename category <old> <new>");
            Console.WriteLine("Remove <id>");
            Console.WriteLine("Stats month <yyyy-MM>");
            Console.WriteLine("Stats yearly <yyyy>");
            Console.WriteLine("Export json <path>");
            Console.WriteLine("Export cvs <path>"); // prompt for overwritting
            Console.WriteLine("Help");
            Console.WriteLine("Exit");
        }

        public static bool GetCommand(out ConsoleCommands command, out string[] args)
        {
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                command = default;
                args = Array.Empty<string>();
                return false;
            }

            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                command = default;
                args = Array.Empty<string>();
                return false;
            }
            var cmdText = parts[0];
            if (cmdText.Equals("by", StringComparison.OrdinalIgnoreCase))
            {
                if (parts[1].Equals("category", StringComparison.OrdinalIgnoreCase))
                {
                    cmdText = "bycategory";
                }
                args = parts.Skip(2).ToArray();

            }
            else
            if (cmdText.Equals("set", StringComparison.OrdinalIgnoreCase))
            {
                if (parts[1].Equals("category", StringComparison.OrdinalIgnoreCase))
                {
                    cmdText = "setcategory";
                }
                args = parts.Skip(2).ToArray();

            }
            else
            if (cmdText.Equals("rename", StringComparison.OrdinalIgnoreCase))
            {
                if (parts[1].Equals("category", StringComparison.OrdinalIgnoreCase))
                {
                    cmdText = "renamecategory";
                }
                args = parts.Skip(2).ToArray();

            }
            else
            {
                args = parts.Skip(1).ToArray();

            }

            if (!Enum.TryParse(cmdText, true, out command))
            {
                Console.WriteLine("Unknown command.");
                return false;
            }
            return true;
        }
        public static async Task Import(string[]? argText, IRepository<Transaction, string> repo, CancellationToken token)
        {
            CSVImporter importer = new(repo);
            if (argText?.Length < 1)
            {
                Logger.Warn("Improper usage of import.");
            }
            try
            {
                await importer.ReadAllFilesAsync(argText!, token);
            }
            catch (OperationCanceledException)
            {
                Logger.Info("Operation Canceled.");
            }
        }

        public static void List(string[]? argText, IRepository<Transaction, string> repo)
        {
            if (argText?.Length < 1)
            {
                Logger.Warn("Improper usage of list.");
                return;
            }

            if (!Enum.TryParse<ListScope>(argText![0], ignoreCase: true, out var scope))
            {
                Logger.Warn("Improper usage of list.");
                return;
            }

            switch (scope)
            {
                case ListScope.All:
                    {
                        var all = repo.GetAll();
                        PrintTransactions(all);
                        break;
                    }
                case ListScope.Month:
                    {
                        var all = repo.GetAll();
                        var monthlyTransactions = all.Where(t => t.Timestamp.MonthKey().Equals(argText[1]));
                        PrintTransactions(monthlyTransactions);
                        break;
                    }
            }
        }

        public static void Over(string[]? argText, IRepository<Transaction, string> repo)
        {
            if (argText?.Length < 1)
            {
                Logger.Warn("Improper usage of over.");
                return;
            }

            if (!decimal.TryParse(argText?[0], out decimal amount))
            {
                Logger.Warn("Amount is not a number");
                return;
            }

            var all = repo.GetAll();
            var overAmountTransactions = all.Where(t => t.Amount >= amount);

            PrintTransactions(overAmountTransactions);

        }

        public static void ByCategory(string?[] argText, IRepository<Transaction, string> repo)
        {
            if (argText?.Length < 1)
            {
                Logger.Warn("Improper usage of by category.");
                return;
            }

            var all = repo.GetAll();
            var byCategoryTransactions = all.Where(t => string.Equals(t.Category, argText![0], StringComparison.OrdinalIgnoreCase));
            PrintTransactions(byCategoryTransactions);
        }

        public static void Search(string?[] argText, IRepository<Transaction, string> repo)
        {
            // sa fie fix pe fix ?
            // ai grija daca cauti mai multe cuvinte
            if (argText?.Length < 1)
            {
                Logger.Warn("Improper usage of search.");
                return;
            }

            var all = repo.GetAll();
            var searchTransactions =
            all.Where(t => string.Equals(t.Payee, argText![0], StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(t.Category, argText![0], StringComparison.OrdinalIgnoreCase));
            PrintTransactions(searchTransactions);
        }

        public static void SetCategory(string?[] argText, IRepository<Transaction, string> repo)
        {
            if (argText == null || argText?.Length < 2)
            {
                Logger.Warn("Improper usage of set category.");
                return;
            }

            var id = argText![0];
            if (!repo.Contains(id!))
            {
                Logger.Warn("Id not found");
                return;
            }
            var newCategoryName = argText![1];

            if (newCategoryName == null)
            {
                Logger.Warn("You must introduce a new category.");
                return;
            }

            if (!repo.TryGet(id!, out Transaction? transaction))
            {
                Logger.Warn("Transaction not found");
                return;
            }
            else
            {
                if (transaction == null)
                {
                    Logger.Warn("There is no transaction for that id.");
                    return;
                }
                else
                {
                    transaction.Category = newCategoryName;
                    Logger.Info("Transaction category succesfully changed to: " + newCategoryName);
                    PrintTransactions(transaction);
                }
            }

        }

        public static void RenameCategory(string?[] argText, IRepository<Transaction, string> repo)
        {
            if (argText?.Length < 2)
            {
                Logger.Warn("Improper usage of rename category.");
                return;
            }
            var all = repo.GetAll();
            var oldCategoryName = argText![0];
            var transactions = all.Where(t => string.Equals(t.Category, oldCategoryName, StringComparison.OrdinalIgnoreCase));
            if (!transactions.Any())
            {
                Logger.Warn("Category not found");
                return;
            }

            var newCategoryName = argText![1];

            if (newCategoryName == null)
            {
                Logger.Warn("You must introduce a new category.");
                return;
            }

            foreach (Transaction t in transactions)
            {
                t.Category = newCategoryName;
            }

            Logger.Info($"Category name changed succesfully from {oldCategoryName} to {newCategoryName}");
            PrintTransactions(transactions); // bizar aici nu mai sunt tranzactii

        }

        public static void Remove(string?[] argText, IRepository<Transaction, string> repo)
        {
            if (argText == null || argText?.Length < 1)
            {
                Logger.Warn("Improper usage of remove.");
                return;
            }
            var id = argText![0];
            if (id == null)
            {
                Logger.Warn("Id must not be null.");
                return;
            }
            if (!repo.Contains(id))
            {
                Logger.Warn("Id not found");
                return;
            }
            else
            {
                repo.Remove(id);
                Logger.Info("Transaction successfully deleted.");
            }

        }

        public static void Stats(string?[] argText, IRepository<Transaction, string> repo)
        {

        }
        public static void PrintTransactions(IEnumerable<Transaction> transactions)
        {
            if (!transactions.Any())
            {
                Logger.Info(Info.NO_TRANSACTIONS_FOUND);
            }

            foreach (var t in transactions)
            {
                Console.WriteLine(t);
            }
        }
        private static void PrintTransactions(Transaction transaction)
        {
            if (transaction == null)
            {
                Logger.Info(Info.NO_TRANSACTIONS_FOUND);
            }

            Console.WriteLine(transaction);

        }
    }
}