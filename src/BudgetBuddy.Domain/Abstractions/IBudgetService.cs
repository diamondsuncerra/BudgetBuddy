using System.Runtime.InteropServices;
using TransactionListResult =
    BudgetBuddy.Domain.Result<
        System.Collections.Generic.IReadOnlyList<BudgetBuddy.Domain.Transaction>>;

namespace BudgetBuddy.Domain.Abstractions
{
    public interface IBudgetService
    {
        TransactionListResult ListMonth(string monthKey);
        TransactionListResult ListAll();
        TransactionListResult OverAmount(decimal amount);
        TransactionListResult ByCategory(string category);
        TransactionListResult Search(string[] terms);

        Result<Transaction> SetCategory(string id, string newCategory);
        TransactionListResult RenameCategory(string oldCategoryName, string newCategoryName);
        Result<bool> Remove(string id);
        Task<Result<bool>> Export(string fileName, string format, bool overwrite, CancellationToken token);
        Task Import(string[] fileName, CancellationToken token);
        public Result<MonthlyFinancialSummary> GetMonthlyFinancialSummary(string month, int top);
        public Result<YearlyFinancialSummary> GetYearlyFinancialSummary(string month, int top);

    }
}