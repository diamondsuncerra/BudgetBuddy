using System.Text;

namespace BudgetBuddy.Domain
{
    public sealed class MonthlyFinancialSummary
    {
        public string MonthKey { get; }
        public decimal Income { get; }
        public decimal Expense { get; }
        public decimal Net => Income - Expense;
        public decimal AverageTransactionSize { get; }
        public IReadOnlyList<TopCategory> TopExpenseCategories { get; }

        public MonthlyFinancialSummary(
            string monthKey,
            decimal income,
            decimal expense,
            decimal averageTransactionSize,
            IReadOnlyList<TopCategory> topExpenseCategories)
        {
            MonthKey = monthKey;
            Income = income;
            Expense = expense;
            AverageTransactionSize = averageTransactionSize;
            TopExpenseCategories = topExpenseCategories;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine($"Month: {MonthKey}");
            builder.AppendLine($"   Income:  {Income}");
            builder.AppendLine($"   Expense: {Expense}");
            builder.AppendLine($"   Net:     {Net}");
            builder.AppendLine($"   Average transaction size: {AverageTransactionSize}");

            if (TopExpenseCategories.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("   Top Expense Categories:");

                foreach (var cat in TopExpenseCategories)
                {
                    builder.AppendLine($"      - {cat.Category}: {cat.Total}");
                }
            }
            else
            {
                builder.AppendLine();
                builder.AppendLine("   No expense categories found.");
            }

            return builder.ToString();
        }

    }

    public sealed class TopCategory
    {
        public string Category { get; }
        public decimal Total { get; }

        public TopCategory(string category, decimal total)
        {
            Category = category;
            Total = total;
        }
    }
}
