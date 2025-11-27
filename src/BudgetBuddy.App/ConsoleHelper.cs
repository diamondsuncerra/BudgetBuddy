using BudgetBuddy.Domain.Extensions;
using BudgetBuddy.Domain;
using BudgetBuddy.Domain.Abstractions;
using TransactionListResult =
    BudgetBuddy.Domain.Result<
        System.Collections.Generic.IReadOnlyList<BudgetBuddy.Domain.Transaction>>;

namespace BudgetBuddy.App
{
    public class ConsoleHelper
    {   // trebuie spart
        // remove sa fie doar remove(id) spre ex
        //private IRepository<Transaction, string> _repository;
        private ILogger _logger;
        private IBudgetService _budgetService;

        public ConsoleHelper(IBudgetService budgetService, ILogger logger)
        {
            _budgetService = budgetService;
            _logger = logger;
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
                await _budgetService.Import(argText, cts.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.Info(Info.CancellingMessage);
            }
            finally
            {
                Console.CancelKeyPress -= handler;
            }
        }

        public void ListAll()
        {
            TransactionListResult result = _budgetService.ListAll();
            if (result.IsSuccess)
            {
                PrintTransactions(result.Value);
            }
            else
            {
                _logger.Error(result.Error);
            }
        }
        public void ListMonth(string[]? argText)
        {
            if (!IsArgsCorrect(argText, 1, ProperUsage.List))
                return;

            var monthResult = argText![0].TryMonth();
            if (!monthResult.IsSuccess)
            {
                _logger.Warn(Warnings.InvalidMonth);
                return;
            }

            var result = _budgetService.ListMonth(monthResult.Value.MonthKey());

            if (!result.IsSuccess)
            {
                _logger.Error(result.Error);
                return;
            }

            PrintTransactions(result.Value!); // TODO maybe make value non-nullable
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

            var result = _budgetService.OverAmount(amount);

            if (!result.IsSuccess)
            {
                _logger.Error(result.Error);
                return;
            }
            PrintTransactions(result.Value!);
        }

        public void ByCategory(string[]? argText)
        {
            if (!IsArgsCorrect(argText, 1, ProperUsage.ByCategory))
                return;

            var category = argText![0];
            var result = _budgetService.ByCategory(category);

            if (!result.IsSuccess)
            {
                _logger.Error(result.Error);
                return;
            }
            PrintTransactions(result.Value!);
        }
        public void Search(string[]? argText)
        {
            if (!IsArgsCorrect(argText, 1, ProperUsage.Search))
                return;

            var searchTerms = argText; // problema aici cu cati sunt we'll see
            var result = _budgetService.Search(searchTerms);
            if (!result.IsSuccess)
            {
                _logger.Error(result.Error);
                return;
            }
            PrintTransactions(result.Value!);
        }
        public async Task Export(string[]? argText)
        {
            if (!IsArgsCorrect(argText, 2, ProperUsage.Export))
                return;

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
                var result = await _budgetService.Export(fileName, format, overwrite, cts.Token);
                if (result.IsSuccess)
                {
                    if (!overwrite)
                        return; // not sure
                    _logger.Info($"Successfully exported data to file {fileName}, in format {argText[0]}.");
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Info(Info.ExportCancelledByUser);
            }
            finally
            {
                Console.CancelKeyPress -= handler;
            }
        }
        public void SetCategory(string[]? argText)
        {
            if (!IsArgsCorrect(argText, 2, ProperUsage.SetCategory))
                return;
            var id = argText[0];
            if (id == null)
            {
                _logger.Warn(Warnings.NullId);
                return;
            }
            var newCategoryName = argText[1];
            if (newCategoryName == null)
            {
                _logger.Warn(Warnings.NullNewCategory);
                return;
            }
            var result = _budgetService.SetCategory(id, newCategoryName);
            if (!result.IsSuccess)
            {
                _logger.Error(result.Error);
                return;
            }
            PrintTransactions(result.Value!);

        }
        public void RenameCategory(string[]? argText)
        {
            if (!IsArgsCorrect(argText, 2, ProperUsage.RenameCategory))
                return;

            var oldCategoryName = argText![0];
            var newCategoryName = argText![1];

            if (newCategoryName == null)
            {
                _logger.Warn(Warnings.NullNewCategory);
                return;
            }
            TransactionListResult result = _budgetService.RenameCategory(oldCategoryName, newCategoryName);
            if (result.IsSuccess)
            {
                _logger.Success($"Category name changed  from {oldCategoryName} to {newCategoryName} for {result.Value!.Count} records.");
                PrintTransactions(result.Value);
            }
            else
            {
                _logger.Error(result.Error);
            }
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
            var result = _budgetService.Remove(id);
            if (result.IsSuccess)
                _logger.Success(Codes.Success);
            else
                _logger.Error(Codes.NotFound);
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
            var result = _budgetService.GetYearlyFinancialSummary(year, 3);
            if (result.IsSuccess)
            {
                Console.WriteLine(result.Value);
            }
            else
            {
                _logger.Error(result.Error);
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
            var result = _budgetService.GetMonthlyFinancialSummary(month, 3);
            if (result.IsSuccess)
            {
                Console.WriteLine(result.Value);
            }
            else
            {
                _logger.Error(result.Error);
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
        public void PrintTransactions(Transaction transaction)
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
            Console.Write(Info.OverWriteQuestionPrompt);
            var response = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(response))
                return false;

            response = response.Trim().ToLowerInvariant();
            return response == "y" || response == "yes";
        }

        private bool IsArgsCorrect(string[]? args, int correctLength, string usage)
        {
            if (args is null || args.Length != correctLength)
            {
                _logger.Warn(usage.ProperUsageFormat());
                return false;
            }

            if (args.Take(correctLength).Any(a => string.IsNullOrWhiteSpace(a)))
            {
                _logger.Warn(usage.ProperUsageFormat());
                return false;
            }

            return true;
        }

        private static bool IsValidExportFormat(string format)
        {
            return Enum.TryParse(typeof(ExportFormat), format, true, out _); // _ -> pt ca nu folosesc!! <3 <3
        }

    }
}