using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BudgetBuddy.Domain;
using BudgetBuddy.Infrastructure.Export;
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
                        if (!argText[1].TryMonth().IsSuccess)
                        {
                            Logger.Warn("Invalid Month");
                            return;
                        }
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
            if (argText == null)
            {
                Logger.Warn("Improper usage of stats.");
                return;
            }
            if (argText.Length < 2)
            {
                Logger.Warn("Improper usage of stats.");
                return;
            }

            if (!Enum.TryParse<StastsScope>(argText[0], ignoreCase: true, out var scope))
            {
                Logger.Warn("Improper usage of stats.");
                return;
            }

            switch (scope)
            {
                case StastsScope.Month:
                    {
                        var month = argText[1];
                        if (string.IsNullOrWhiteSpace(month))
                        {
                            Logger.Warn("No month given.");
                            return;
                        }

                        if (!month.TryMonth().IsSuccess)
                        {
                            Logger.Warn("Not a valid date.");
                            return;
                        }
                        (decimal income, decimal expense, decimal net) = GetIncomeExpenseNetForMonth(month, repo);
                        decimal averageTransactionSize = GetAverageTransactionSizeForMonth(month, repo);
                        IEnumerable<(string, decimal)> topCategories = GetTopExpenseCategoriesForMonth(month, repo, 3);
                        
                        Logger.Info($"For the month {month}");
                        Logger.Info($"Income: {income}, Expense: {expense}, Net: {net}");
                        Logger.Info($"Average size of a transaction was: {averageTransactionSize}");
                        Logger.Info(topCategories.ToPrettyTable("USD"));

                        break;
                    }
                case StastsScope.Yearly:
                    {
                        var all = repo.GetAll();
                        var monthlyTransactions = all.Where(t => t.Timestamp.MonthKey().Equals(argText[1]));
                        PrintTransactions(monthlyTransactions);
                        break;
                    }
            }
        }

        public static async Task Export(string[]? argText, IRepository<Transaction, string> repo, CancellationToken token)
        {

            if (argText == null)
            {
                Logger.Warn("Improper usage of export.");
                return;
            }

            if (argText.Length < 2)
            {
                Logger.Warn("Improper usage of export.");
                return;

            }

            if (string.IsNullOrWhiteSpace(argText[0]))
            {
                Logger.Warn("Improper usage of export.");
                return;
            }

            if (string.IsNullOrWhiteSpace(argText[1]))
            {
                Logger.Warn("Improper usage of export.");
                return;
            }

            if (repo.Count() == 0)
            {
                Logger.Info("No data found to export.");
                return;
            }

            IExportStrategy strategy;
            string fileName = argText[1];

            if (argText[0].Equals("json", StringComparison.OrdinalIgnoreCase))
            {
                strategy = new JsonExportStrategy();
            }
            else
            if (argText[0].Equals("csv", StringComparison.OrdinalIgnoreCase))
            {
                strategy = new CvsExportStrategy();
            }
            else
            {
                Logger.Warn("The strategy is not implemented yet. Choose between json or csv.");
                return;
            }

            Exporter exporter = new Exporter(strategy);
            bool overwrite = ConfirmOverwrite(fileName);
            bool result = await exporter.Run(fileName, repo.GetAll(), token, overwrite);

            if (result)
            {
                if (overwrite) return;
                Logger.Info($"Succesfully exported data to file {fileName}, in format {argText[0]}.");
            }
            else
            {
                Logger.Warn("Export failed.");
            }
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

        public static bool ConfirmOverwrite(string fileName)
        {
            if (!File.Exists(fileName))
                return true;

            Console.WriteLine($"File {fileName} already exists.");
            Console.Write("Do you want to overwrite it? [y/N]: ");
            var response = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(response))
                return false;

            response = response.Trim().ToLowerInvariant();
            return response == "y" || response == "yes";
        }


        public static (decimal, decimal, decimal) GetIncomeExpenseNetForMonth(string month, IRepository<Transaction, string> repo)
        {
            decimal income = 0m, expense = 0m;
            var all = repo.GetAll();
            var monthlyTransactions = all.Where(t => t.Timestamp.MonthKey().Equals(month));

            var incomeTransactions = monthlyTransactions.Where(t => t.Amount > 0m).Select(t => t.Amount);
            var expenseTransactions = monthlyTransactions.Where(t => t.Amount <= 0m).Select(t => t.Amount);

            income = incomeTransactions.Sum();
            expense = expenseTransactions.SumAbs();

            decimal net = income - expense;
            return (income, expense, net);
        }

        public static decimal GetAverageTransactionSizeForMonth(string month, IRepository<Transaction, string> repo)
        {
            var all = repo.GetAll();
            var monthly = all.Where(t => t.Timestamp.MonthKey().Equals(month)).Select(t=>t.Amount);
            return monthly.Any() ? monthly.AverageAbs() : 0m;
        }

        public static IEnumerable<(string Category, decimal Total)> GetTopExpenseCategoriesForMonth(string month, IRepository<Transaction, string> repo, int top)
        {
            var all = repo.GetAll();
            var monthlyTransactions = all.Where(t => t.Timestamp.MonthKey().Equals(month));
            return monthlyTransactions.Where(t => t.Amount < 0m)
            .GroupBy(t => t.Category)
            .Select(g => (Category: g.Key, Total: g.Select(t => t.Amount).Sum()))
            .OrderByDescending(x => -x.Total)
            .Take(top);

        }
    }
}