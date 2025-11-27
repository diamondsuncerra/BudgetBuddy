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

        public (decimal, decimal, decimal) GetIncomeExpenseNetForMonth(string month)
        {   // TODO: No more tuples
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

        public async Task Export(string fileName, string format, CancellationToken token, bool overwrite)
        {
            await _exportService.Export(fileName, format, _repository.GetAll(), token, overwrite);
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

        public Result<bool> SetCategory(string id, string newCategory, out Transaction? updated)
        {
            throw new NotImplementedException();
        }
    }
}