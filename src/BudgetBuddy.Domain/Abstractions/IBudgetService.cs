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

        Result<bool> SetCategory(string id, string newCategory, out Transaction? updated);
        TransactionListResult RenameCategory(string oldCategoryName, string newCategoryName);
        Result<bool> Remove(string id);

        bool YearExists(string year);
        bool MonthExists(string month);

        (decimal income, decimal expense, decimal net) GetIncomeExpenseNetForMonth(string month);
        decimal GetAverageTransactionSizeForMonth(string month);
        IEnumerable<(string Category, decimal Total)> GetTopExpenseCategoriesForMonth(string month, int top);
    }
}