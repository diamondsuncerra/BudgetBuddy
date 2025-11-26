using BudgetBuddy.Domain.Extensions;
using BudgetBuddy.Domain;
using BudgetBuddy.Domain.Abstractions;

namespace BudgetBuddy.App
{
    public class ConsoleHelper
    {
        private IRepository<Transaction, string> _repository;
        private ILogger _logger;
        private IImporter _importer;
        private IExportService _exportService;

        public ConsoleHelper(IRepository<Transaction, string> repository, ILogger logger, IImporter importer, IExportService exportService)
        {
            _repository = repository;
            _logger = logger;
            _importer = importer;
            _exportService = exportService;
        }
        public void PrintAllOptions()
        {
            Console.WriteLine(ProperUsage.Import);
            Console.WriteLine(ProperUsage.List);
            Console.WriteLine(ProperUsage.ByCategory);
            Console.WriteLine(ProperUsage.Over);
            Console.WriteLine(ProperUsage.Search);
            Console.WriteLine(ProperUsage.RenameCategory);
            Console.WriteLine(ProperUsage.SetCategory);
            Console.WriteLine(ProperUsage.Remove);
            Console.WriteLine(ProperUsage.Stats);
            Console.WriteLine(ProperUsage.Export);
            Console.WriteLine(ProperUsage.Help);
            Console.WriteLine(ProperUsage.Exit);
        }

        public bool GetCommand(out ConsoleCommands command, out string[] args)
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

        public async Task Import(string[]? argText)
        {
            if (!IsArgsCorrect(argText, 1, ProperUsage.Import))
                return;

            using var cts = new CancellationTokenSource();

            ConsoleCancelEventHandler? handler = (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            Console.CancelKeyPress += handler;

            try
            {

                await _importer.ReadAllFilesAsync(argText!, cts.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.Info(Info.CancellingMessage);
            }
            finally
            {
                // Ca sa pot relua importul
                Console.CancelKeyPress -= handler;
            }
        }


        public void ListAll()
        {
            var all = _repository.GetAll();
            PrintTransactions(all);
        }
        public void ListMonth(string[]? argText)
        {
            if (!IsArgsCorrect(argText, 1, ProperUsage.List))
                return;

            if (argText![0].TryMonth().IsSuccess)
            {
                var all = _repository.GetAll();
                var monthlyTransactions = all.Where(t => t.Timestamp.MonthKey().Equals(argText![0]));
                PrintTransactions(monthlyTransactions);
            }
            else
            {
                _logger.Warn(Warnings.InvalidMonth);
            }

        }

        public void Over(string[]? argText)
        {
            if (!IsArgsCorrect(argText, 1, ProperUsage.Over))
                return;

            if (!decimal.TryParse(argText?[0], out decimal amount))
            {
                _logger.Warn(Warnings.InvalidAmount);
                return;
            }

            if (amount <= 0)
            {
                _logger.Warn(Warnings.InvalidAmount);
                return;
            }

            var all = _repository.GetAll();
            var overAmountTransactions = all.Where(t => t.Amount <= -amount && t.Amount < 0);
            PrintTransactions(overAmountTransactions);
        }

        public void ByCategory(string[]? argText)
        {
            if (!IsArgsCorrect(argText, 1, ProperUsage.ByCategory))
                return;

            var all = _repository.GetAll();
            var byCategoryTransactions = all.Where(t => string.Equals(t.Category, argText![0], StringComparison.OrdinalIgnoreCase));
            PrintTransactions(byCategoryTransactions);
        }

        public void Search(string[]? argText)
        {
            if (!IsArgsCorrect(argText, 1, ProperUsage.Search))
                return;

            var all = _repository.GetAll();
            var hits = _repository.GetAll().Where(t =>
            argText!.Any(term =>
            t.Payee.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            t.Category.Contains(term, StringComparison.OrdinalIgnoreCase)));

            PrintTransactions(hits);
        }

        public void SetCategory(string[]? argText)
        {
            if (!IsArgsCorrect(argText, 2, ProperUsage.SetCategory))
                return;

            var id = argText![0];
            if (!_repository.Contains(id!))
            {
                _logger.Error(Codes.NotFound);
                return;
            }
            var newCategoryName = argText![1];

            if (newCategoryName == null)
            {
                _logger.Warn(Warnings.NullNewCategory);
                return;
            }

            if (!_repository.TryGet(id!, out Transaction? transaction))
            {
                _logger.Warn(Warnings.TransactionNotFound);
                return;
            }
            else
            {
                if (transaction == null)
                {
                    _logger.Warn(Warnings.TransactionNotFound);
                    return;
                }
                else
                {
                    transaction.Category = newCategoryName;
                    _logger.Success(Codes.Success);
                    _logger.Success("Transaction category changed to: " + newCategoryName);
                    PrintTransactions(transaction);
                }
            }

        }

        public void RenameCategory(string[]? argText)
        {
            if (!IsArgsCorrect(argText, 2, ProperUsage.RenameCategory))
                return;

            var all = _repository.GetAll();
            var oldCategoryName = argText![0];
            var transactions = all.Where(t => string.Equals(t.Category, oldCategoryName, StringComparison.OrdinalIgnoreCase));

            if (!transactions.Any())
            {
                _logger.Warn(Warnings.CategoryNotFound);
                return;
            }

            var newCategoryName = argText![1];
            var transactionCount = transactions.Count();
            if (newCategoryName == null)
            {
                _logger.Warn(Warnings.NullNewCategory);
                return;
            }

            foreach (Transaction t in transactions)
            {
                t.Category = newCategoryName;
            }

            _logger.Success($"Category name changed  from {oldCategoryName} to {newCategoryName} for {transactionCount} records.");
            PrintTransactions(all.Where(t => string.Equals(t.Category, newCategoryName, StringComparison.OrdinalIgnoreCase)));

        }

        public void Remove(string[]? argText)
        {
            if (!IsArgsCorrect(argText, 1, ProperUsage.Remove))
                return;

            var id = argText![0];
            if (id == null)
            {
                _logger.Warn(Warnings.NullId);
                return;
            }
            if (!_repository.Contains(id))
            {
                _logger.Error(Codes.NotFound);
                return;
            }
            else
            {
                _repository.Remove(id);
                _logger.Success(Codes.Success);
            }

        }

        public void StatsYearly(string[]? argText)
        {
            if (!IsArgsCorrect(argText, 1, ProperUsage.Stats))
                return;

            var year = argText![0];
            if (string.IsNullOrWhiteSpace(year))
            {
                _logger.Warn(Warnings.NoYearGiven);
                return;
            }

            if (!year.TryYear().IsSuccess)
            {
                _logger.Warn(Warnings.InvalidYear);
                return;
            }

            if (!IsYearInTheSystem(year))
            {
                _logger.Warn(Warnings.YearNotFound);
                return;
            }

            for (int i = 1; i <= 12; i++)
            {
                var month = year.ToMonthAndYear(i);
                if (!IsMonthInTheSystem(month))
                {
                    _logger.Info("No data for the month: " + month);
                }
                else
                {
                    GetMonthlyStats(month);
                }
            }
        }
        public void StatsMonth(string[]? argText)
        {
            if (!IsArgsCorrect(argText, 1, ProperUsage.Stats))
                return;
            var month = argText![0];
            if (string.IsNullOrWhiteSpace(month))
            {
                _logger.Warn(Warnings.NoMonthGiven);
                return;
            }

            if (!month.TryMonth().IsSuccess)
            {
                _logger.Warn(Warnings.InvalidDate);
                return;
            }

            if (!IsMonthInTheSystem(month))
            {
                _logger.Warn(Warnings.MonthNotFound);
                return;
            }
            GetMonthlyStats(month);
        }

        private void GetMonthlyStats(string month)
        {
            (decimal income, decimal expense, decimal net) = GetIncomeExpenseNetForMonth(month);
            decimal averageTransactionSize = GetAverageTransactionSizeForMonth(month);
            IEnumerable<(string, decimal)> topCategories = GetTopExpenseCategoriesForMonth(month, 3);

            _logger.Info($"For the month {month}");
            _logger.Info($"Income: {income}, Expense: {expense}, Net: {net}");
            _logger.Info($"Average size of a transaction was: {averageTransactionSize}");
            _logger.Info(topCategories.ToPrettyTable("USD"));
        }
        public async Task Export(string[]? argText)
        {

            if (!IsArgsCorrect(argText, 2, ProperUsage.Export))
                return;


            if (_repository.Count() == 0)
            {
                _logger.Info("No data found to export.");
                return;
            }
            string fileName = argText![1];

            if (!IsValidExportFormat(argText[0]))
            {
                _logger.Warn(Warnings.InvalidExportFormat);
                return;
            }
            var format = argText[0];
            using var cts = new CancellationTokenSource();

            ConsoleCancelEventHandler? handler = (s, e) =>
            {
                e.Cancel = true;
                _logger.Warn("Cancelling export.");
                cts.Cancel();

            };

            Console.CancelKeyPress += handler;

            try
            {
                bool overwrite = ConfirmOverwrite(fileName);
                bool result = await _exportService.Export(fileName, format, _repository.GetAll(), cts.Token, overwrite);
                if (result)
                {
                    if (!overwrite)
                        return; // not sure
                    _logger.Info($"Successfully exported data to file {fileName}, in format {argText[0]}.");
                }
                else
                {
                    _logger.Error("Export failed.");
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Info("Export cancelled by user.");
            }
            finally
            {
                Console.CancelKeyPress -= handler;
            }
        }
        public void PrintTransactions(IEnumerable<Transaction> transactions)
        {
            if (!transactions.Any())
            {
                _logger.Info(Info.NoTransactionsFound);
                return;
            }

            foreach (var t in transactions)
            {
                Console.WriteLine(t);
            }
        }
        private void PrintTransactions(Transaction transaction)
        {
            if (transaction == null)
            {
                _logger.Info(Info.NoTransactionsFound);
                return;
            }

            Console.WriteLine(transaction);

        }

        public bool ConfirmOverwrite(string fileName)
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


        public (decimal, decimal, decimal) GetIncomeExpenseNetForMonth(string month)
        {
            decimal income = 0m, expense = 0m;
            var all = _repository.GetAll();
            var monthlyTransactions = all.Where(t => t.Timestamp.MonthKey().Equals(month));

            var incomeTransactions = monthlyTransactions.Where(t => t.Amount > 0m).Select(t => t.Amount);
            var expenseTransactions = monthlyTransactions.Where(t => t.Amount <= 0m).Select(t => t.Amount);

            income = incomeTransactions.Sum();
            expense = expenseTransactions.SumAbs();

            decimal net = income - expense;
            return (income, expense, net);
        }

        public decimal GetAverageTransactionSizeForMonth(string month)
        {
            var all = _repository.GetAll();
            var monthly = all.Where(t => t.Timestamp.MonthKey().Equals(month)).Select(t => t.Amount);
            return monthly.Any() ? monthly.AverageAbs() : 0m;
        }

        public IEnumerable<(string Category, decimal Total)> GetTopExpenseCategoriesForMonth(string month, int top)
        {
            var all = _repository.GetAll();
            var monthlyTransactions = all.Where(t => t.Timestamp.MonthKey().Equals(month));
            return monthlyTransactions.Where(t => t.Amount < 0m)
            .GroupBy(t => t.Category)
            .Select(g => (Category: g.Key, Total: g.Select(t => t.Amount).Sum()))
            .OrderByDescending(x => -x.Total)
            .Take(top);

        }

        private bool IsArgsCorrect(string[]? args, int correctLength, string usage)
        {
            if (args is null || args.Length != correctLength)
            {
                _logger.Warn($"Improper usage. Try: {usage}.");
                return false;
            }

            if (args.Take(correctLength).Any(a => string.IsNullOrWhiteSpace(a)))
            {
                _logger.Warn($"Improper usage. Try: {usage}.");
                return false;
            }

            return true;
        }

        private static bool IsValidExportFormat(string format)
        {
            return Enum.TryParse(typeof(ExportFormat), format, true, out _); // _ -> pt ca nu folosesc!! <3 <3
        }
        private bool IsYearInTheSystem(string year)
        {
            var transactions = _repository.GetAll();
            return transactions.Where(t => t.Timestamp.YearKey().Equals(year)).Any();
        }

        private bool IsMonthInTheSystem(string month)
        {
            var transactions = _repository.GetAll();
            return transactions.Where(t => t.Timestamp.MonthKey().Equals(month)).Any();
        }
    }
}