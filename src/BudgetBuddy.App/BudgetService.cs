using BudgetBuddy.Domain.Abstractions;
using BudgetBuddy.Domain.Extensions;
using BudgetBuddy.Domain;
using TransactionListResult =
    BudgetBuddy.Domain.Result<
        System.Collections.Generic.IReadOnlyList<BudgetBuddy.Domain.Transaction>>;
namespace BudgetBuddy.App
{
    public class BudgetService : IBudgetService
    {
        private IRepository<Transaction, string> _repository;
        //private IImportService _importer;
        private IExportService _exportService;

        public BudgetService(IRepository<Transaction, string> repository, IExportService exportService)
        {
            _repository = repository;
            _exportService = exportService;
        }
        public TransactionListResult ListAll()
        {
            var repositoryList = _repository.GetAll().ToList();
            if(repositoryList.Count == 0)
            {
                return TransactionListResult.Fail("No transactions found.");
            } 
            return TransactionListResult.Ok(repositoryList);
        }

        public TransactionListResult ListMonth(string monthKey)
        {
            var monthlyTransactions = _repository.GetAll().Where(t => t.Timestamp.MonthKey().Equals(monthKey)).ToList();
            if(monthlyTransactions.Count == 0)
            {
                return TransactionListResult.Fail("No transactions found.");
            } 
            return TransactionListResult.Ok(monthlyTransactions);
        }

        public IEnumerable<Transaction> OverAmount(decimal amount)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Transaction> ByCategory(string category)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Transaction> Search(string[] terms)
        {
            throw new NotImplementedException();
        }

        public bool SetCategory(string id, string newCategory, out Transaction? updated)
        {
            throw new NotImplementedException();
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

        public bool Remove(string id)
        {
            return _repository.Remove(id);
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

        TransactionListResult IBudgetService.OverAmount(decimal amount)
        {
            throw new NotImplementedException();
        }

        TransactionListResult IBudgetService.ByCategory(string category)
        {
            throw new NotImplementedException();
        }
    }
}