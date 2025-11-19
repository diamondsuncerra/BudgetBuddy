using BudgetBuddy.Domain;
using BudgetBuddy.Infrastructure.Export;
using BudgetBuddy.Infrastructure.Import;


namespace BudgetBuddy.App
{
    public static class ConsoleHelper
    {
        public static void PrintAllOptions()
        {
            Console.WriteLine(ProperUsage.Import);
            Console.WriteLine(ProperUsage.List);
            Console.WriteLine(ProperUsage.ByCategory);
            Console.WriteLine(ProperUsage.Over);
            Console.WriteLine(ProperUsage.Search);
            Console.WriteLine(ProperUsage.RenameCategory);
            Console.WriteLine(ProperUsage.SetCategory);
            Console.WriteLine(ProperUsage.Remove); // TOOD EXIT CODE 200 404    
            Console.WriteLine(ProperUsage.Stats);
            Console.WriteLine(ProperUsage.Export); // date time are si ora
            Console.WriteLine(ProperUsage.Help);
            Console.WriteLine(ProperUsage.Exit);
        }

        public static bool GetCommand(out ConsoleCommands command, out string[] args)
        {
            command = default;
            args = Array.Empty<string>();

            var line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
                return false;

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return false;

            // Normalize 1-word and 2-word command keys
            static string Key(string a, string? b = null)
                => b is null ? a.ToLowerInvariant() : $"{a.ToLowerInvariant()} {b.ToLowerInvariant()}";


            var map = new Dictionary<string, ConsoleCommands>(StringComparer.OrdinalIgnoreCase)
            {
                ["import"] = ConsoleCommands.Import,
                ["list all"] = ConsoleCommands.ListAll,
                ["list month"] = ConsoleCommands.ListMonth,
                ["by category"] = ConsoleCommands.ByCategory,
                ["over"] = ConsoleCommands.Over,
                ["search"] = ConsoleCommands.Search,
                ["set category"] = ConsoleCommands.SetCategory,
                ["rename category"] = ConsoleCommands.RenameCategory,
                ["remove"] = ConsoleCommands.Remove,
                ["stats month"] = ConsoleCommands.StatsMonth,
                ["stats yearly"] = ConsoleCommands.StatsYearly,
                ["export"] = ConsoleCommands.Export,
                ["help"] = ConsoleCommands.Help,
                ["exit"] = ConsoleCommands.Exit
            };

            var name = parts[0];
            var sub = parts.Length >= 2 ? parts[1] : null;

            // Try 2-word command first
            if (sub != null && map.TryGetValue(Key(name, sub), out command))
            {
                args = parts.Skip(2).ToArray();
                return true;
            }


            if (map.TryGetValue(Key(name), out command))
            {
                args = parts.Skip(1).ToArray();
                return true;
            }

            return false;
        }

        public static async Task Import(string[]? argText, IRepository<Transaction, string> repo, CancellationToken token)
        {
            if (!HasArgs(argText, 1, ProperUsage.Import))
                return;
            try
            {
                CSVImporter importer = new(repo);
                await importer.ReadAllFilesAsync(argText!, token);
            }
            catch (OperationCanceledException)
            {
                Logger.Info(Info.CancellingMessage);
            }
        }

        public static void ListAll(IRepository<Transaction, string> repo)
        {
            var all = repo.GetAll();
            PrintTransactions(all);
        }
        public static void ListMonth(string[]? argText, IRepository<Transaction, string> repo)
        {
            if (!HasArgs(argText, 1, ProperUsage.List))
                return;

            if (argText![0].TryMonth().IsSuccess)
            {
                var all = repo.GetAll();
                var monthlyTransactions = all.Where(t => t.Timestamp.MonthKey().Equals(argText![0]));
                PrintTransactions(monthlyTransactions);
            }
            else
            {
                Logger.Warn(Warnings.InvalidMonth);
            }

        }



        public static void Over(string[]? argText, IRepository<Transaction, string> repo)
        {
            if (!HasArgs(argText, 1, ProperUsage.Over))
                return;

            if (!decimal.TryParse(argText?[0], out decimal amount))
            {
                Logger.Warn(Warnings.InvalidAmount);
                return;
            }

            var all = repo.GetAll();
            var overAmountTransactions = all.Where(t => t.Amount >= amount);
            PrintTransactions(overAmountTransactions);
        }

        public static void ByCategory(string[]? argText, IRepository<Transaction, string> repo)
        {
            if (!HasArgs(argText, 1, ProperUsage.ByCategory))
                return;

            var all = repo.GetAll();
            var byCategoryTransactions = all.Where(t => string.Equals(t.Category, argText![0], StringComparison.OrdinalIgnoreCase));
            PrintTransactions(byCategoryTransactions);
        }

        public static void Search(string[]? argText, IRepository<Transaction, string> repo)
        {
            if (!HasArgs(argText, 1, ProperUsage.Search))
                return;

            var all = repo.GetAll();
            var hits = repo.GetAll().Where(t =>
            argText!.Any(term =>
            t.Payee.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            t.Category.Contains(term, StringComparison.OrdinalIgnoreCase)));

            PrintTransactions(hits);
        }

        public static void SetCategory(string[]? argText, IRepository<Transaction, string> repo)
        {
            if (!HasArgs(argText, 2, ProperUsage.SetCategory))
                return;

            var id = argText![0];
            if (!repo.Contains(id!))
            {
                Logger.Error(Codes.NotFound);
                return;
            }
            var newCategoryName = argText![1];

            if (newCategoryName == null)
            {
                Logger.Warn(Warnings.NullNewCategory);
                return;
            }

            if (!repo.TryGet(id!, out Transaction? transaction))
            {
                Logger.Warn(Warnings.TransactionNotFound);
                return;
            }
            else
            {
                if (transaction == null)
                {
                    Logger.Warn(Warnings.TransactionNotFound);
                    return;
                }
                else
                {
                    transaction.Category = newCategoryName;
                    Logger.GreenInfo(Codes.Success);
                    Logger.GreenInfo("Transaction category changed to: " + newCategoryName);
                    PrintTransactions(transaction);
                }
            }

        }

        public static void RenameCategory(string[]? argText, IRepository<Transaction, string> repo)
        {
            if (!HasArgs(argText, 2, ProperUsage.RenameCategory))
                return;

            var all = repo.GetAll();
            var oldCategoryName = argText![0];
            var transactions = all.Where(t => string.Equals(t.Category, oldCategoryName, StringComparison.OrdinalIgnoreCase));

            if (!transactions.Any())
            {
                Logger.Warn(Warnings.CategoryNotFound);
                return;
            }

            var newCategoryName = argText![1];
            var transactionCount = transactions.Count();
            if (newCategoryName == null)
            {
                Logger.Warn(Warnings.NullNewCategory);
                return;
            }

            foreach (Transaction t in transactions)
            {
                t.Category = newCategoryName;
            }
            
            Logger.GreenInfo($"Category name changed  from {oldCategoryName} to {newCategoryName} for {transactionCount} records.");
            PrintTransactions(all.Where(t => string.Equals(t.Category, newCategoryName, StringComparison.OrdinalIgnoreCase))); 

        }

        public static void Remove(string[]? argText, IRepository<Transaction, string> repo)
        {
            if (!HasArgs(argText, 1, ProperUsage.Remove))
                return;

            var id = argText![0];
            if (id == null)
            {
                Logger.Warn(Warnings.NullId);
                return;
            }
            if (!repo.Contains(id))
            {
                Logger.Error(Codes.NotFound);
                return;
            }
            else
            {
                repo.Remove(id);
                Logger.GreenInfo(Codes.Success);
            }

        }

        public static void StatsYearly(string[]? argText, IRepository<Transaction, string> repo)
        {
            if (!HasArgs(argText, 1, ProperUsage.Stats))
                return;

            var year = argText![0];
            if (string.IsNullOrWhiteSpace(year))
            {
                Logger.Warn(Warnings.NoYearGiven);
                return;
            }

            if (!year.TryYear().IsSuccess)
            {
                Logger.Warn(Warnings.InvalidYear);
                return;
            }

            for (int i = 1; i <= 12; i++)
            {
                var month = year.ToMonthAndYear(i);
                GetMonthlyStats(month, repo);
            }
        }
        public static void StatsMonth(string[]? argText, IRepository<Transaction, string> repo)
        {
            if (!HasArgs(argText, 1, ProperUsage.Stats))
                return;
            var month = argText![0];
            if (string.IsNullOrWhiteSpace(month))
            {
                Logger.Warn(Warnings.NoMonthGiven);
                return;
            }

            if (!month.TryMonth().IsSuccess)
            {
                Logger.Warn(Warnings.InvalidDate);
                return;
            }
            GetMonthlyStats(month, repo);
        }



        private static void GetMonthlyStats(string month, IRepository<Transaction, string> repo)
        {
            (decimal income, decimal expense, decimal net) = GetIncomeExpenseNetForMonth(month, repo);
            decimal averageTransactionSize = GetAverageTransactionSizeForMonth(month, repo);
            IEnumerable<(string, decimal)> topCategories = GetTopExpenseCategoriesForMonth(month, repo, 3);

            Logger.Info($"For the month {month}");
            Logger.Info($"Income: {income}, Expense: {expense}, Net: {net}");
            Logger.Info($"Average size of a transaction was: {averageTransactionSize}");
            Logger.Info(topCategories.ToPrettyTable("USD"));
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
                strategy = new CsvExportStrategy();
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
                if (overwrite)
                    return;
                Logger.Info($"Succesfully exported data to file {fileName}, in format {argText[0]}.");
            }
            else
            {
                Logger.Error("Export failed.");
            }
        }
        public static void PrintTransactions(IEnumerable<Transaction> transactions)
        {
            if (!transactions.Any())
            {
                Logger.Info(Info.NoTransactionsFound);
                return;
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
                Logger.Info(Info.NoTransactionsFound);
                return;
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
            var monthly = all.Where(t => t.Timestamp.MonthKey().Equals(month)).Select(t => t.Amount);
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

        private static bool HasArgs(string[]? args, int min, string usage)
        {
            if (args is null || args.Length < min)
            {
                Logger.Warn($"Improper usage. Try: {usage}.");
                return false;
            }
            return true;
        }

    }
}