using BudgetBuddy.Domain.Abstractions;
using BudgetBuddy.Domain.Extensions;
using BudgetBuddy.Domain;
using TransactionListResult =
    BudgetBuddy.Domain.Result<
        System.Collections.Generic.IReadOnlyList<BudgetBuddy.Domain.Transaction>>;
using Microsoft.VisualBasic;
namespace BudgetBuddy.App
{
    public class BudgetService : IBudgetService
    {
        private IRepository<Transaction, string> _repository;
        private IImportService _importService;
        private IExportService _exportService;

        public BudgetService(IRepository<Transaction, string> repository, IImportService importService, IExportService exportService)
        {
            _repository = repository;
            _importService = importService;
            _exportService = exportService;
        }
        public TransactionListResult ListAll()
        {
            var allTransactions = _repository.GetAll().ToList();
            return GetResultOrError(allTransactions);
        }

        public TransactionListResult ListMonth(string monthKey)
        {
            var monthlyTransactions = _repository.GetAll().Where(t => t.Timestamp.MonthKey().Equals(monthKey)).ToList();
            return GetResultOrError(monthlyTransactions);
        }


        public TransactionListResult RenameCategory(string oldCategoryName, string newCategoryName)
        {
            // Move to list because it s bad to enumerate multiple times
            // IReadOnlyList because we moved to list
            var all = _repository.GetAll();
            var transactions = all
                .Where(t => string.Equals(t.Category, oldCategoryName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (transactions.Count == 0)
                return TransactionListResult.Fail("No transactions found.");

            foreach (var t in transactions)
                t.Category = newCategoryName;

            return TransactionListResult.Ok(transactions);
        }

        public Result<bool> Remove(string id)
        {
            if (_repository.Remove(id))
                return Result<bool>.Ok(true);
            else
                return Result<bool>.Fail("Id not found.");
        }

        public bool YearExists(string year)
        {
            var transactions = _repository.GetAll();
            return transactions.Where(t => t.Timestamp.YearKey().Equals(year)).Any();
        }

        public bool MonthExists(string month)
        {
            var transactions = _repository.GetAll();
            return transactions.Where(t => t.Timestamp.MonthKey().Equals(month)).Any();
        }

        public async Task Export(string fileName, string format, bool overwrite, CancellationToken token)
        {
            await _exportService.Export(fileName, format, _repository.GetAll(), token, overwrite);
        }
        public TransactionListResult OverAmount(decimal amount)
        {
            var overAmountTransactions = _repository.GetAll().Where(t => t.Amount <= -amount && t.Amount < 0).ToList();
            return GetResultOrError(overAmountTransactions);
        }

        public TransactionListResult ByCategory(string category)
        {
            var transactionsByCategory = _repository.GetAll()
            .Where(t => string.Equals(t.Category, category, StringComparison.OrdinalIgnoreCase))
            .ToList();
            return GetResultOrError(transactionsByCategory);
        }

        private TransactionListResult GetResultOrError(IReadOnlyList<Transaction> transactions)
        {
            if (transactions.Count == 0)
                return TransactionListResult.Fail("No transactions found.");
            else
                return TransactionListResult.Ok(transactions);
        }

        public TransactionListResult Search(string[] terms)
        {
            var searchTransactions = _repository.GetAll().Where(t =>
            terms.Any(term =>
            t.Payee.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            t.Category.Contains(term, StringComparison.OrdinalIgnoreCase))).ToList();
            return GetResultOrError(searchTransactions);
        }

        public Result<Transaction> SetCategory(string id, string newCategoryName)
        {
            if (_repository.TryGet(id, out Transaction? transaction))
            {
                if (transaction == null) // poate id-ul exista dar nu si tranzactie la el
                    return Result<Transaction>.Fail("No transaction found");
                transaction.Category = newCategoryName;
                return Result<Transaction>.Ok(transaction);
            }
            else
                return Result<Transaction>.Fail("No transaction found");
        }

        public async Task Import(string[] fileName, CancellationToken token)
        {
            await _importService.ReadAllFilesAsync(fileName, token);
        }

        public Result<MonthlyFinancialSummary> GetMonthlyFinancialSummary(string month, int top)
        {
            var monthlyTransactions = _repository.GetAll()
                .Where(t => t.Timestamp.MonthKey().Equals(month))
                .ToList();

            if (monthlyTransactions.Count == 0)
                return Result<MonthlyFinancialSummary>.Fail("No transactions found for this month.");

            var incomeTransactions = monthlyTransactions
                .Where(t => t.Amount > 0m)
                .Select(t => t.Amount)
                .ToList();

            var expenseTransactions = monthlyTransactions
                .Where(t => t.Amount <= 0m)
                .Select(t => t.Amount)
                .ToList();

            var income = incomeTransactions.Sum();
            var expense = expenseTransactions.SumAbs();
            var average = monthlyTransactions.Select(t => t.Amount).AverageAbs(); // fixed: only this month

            var topCategories = monthlyTransactions
                .Where(t => t.Amount < 0m)
                .GroupBy(t => t.Category)
                .Select(g => new TopCategory(
                    g.Key,
                    g.Select(t => t.Amount).Sum()
                ))
                .OrderByDescending(x => -x.Total)
                .Take(top)
                .ToList()
                .AsReadOnly();

            var summary = new MonthlyFinancialSummary(
                monthKey: month,
                income: income,
                expense: expense,
                averageTransactionSize: average,
                topExpenseCategories: topCategories
            );

            return Result<MonthlyFinancialSummary>.Ok(summary);
        }

        public Result<YearlyFinancialSummary> GetYearlyFinancialSummary(string year, int topCategories)
        {
            var yearlyFinancialSummary = new List<MonthlyFinancialSummary>();

            for (int month = 1; month <= 12; month++)
            {
                string monthKey = year.ToMonthAndYear(month);
                var monthlyFinancialResult = GetMonthlyFinancialSummary(monthKey, topCategories);

                if (monthlyFinancialResult.IsSuccess)
                {
                    yearlyFinancialSummary.Add(monthlyFinancialResult.Value!);
                }

            }
            if (yearlyFinancialSummary.Count == 0)
                return Result<YearlyFinancialSummary>.Fail("No data found for the entire year.");

            var yearly = new YearlyFinancialSummary(year, yearlyFinancialSummary.AsReadOnly());
            return Result<YearlyFinancialSummary>.Ok(yearly);
        }

    }
}