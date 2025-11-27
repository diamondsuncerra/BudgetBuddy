using System.Text;

namespace BudgetBuddy.Domain
{
    public sealed class YearlyFinancialSummary
    {
        public string Year { get; }
        public IReadOnlyList<MonthlyFinancialSummary> Months { get; }

        public decimal TotalIncome => Months.Sum(m => m.Income);
        public decimal TotalExpense => Months.Sum(m => m.Expense);
        public decimal TotalNet => Months.Sum(m => m.Net);

        public YearlyFinancialSummary(string year, IReadOnlyList<MonthlyFinancialSummary> months)
        {
            Year = year;
            Months = months;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Year: {Year}");
            builder.AppendLine($"Total Income:  {TotalIncome}");
            builder.AppendLine($"Total Expense: {TotalExpense}");
            builder.AppendLine($"Total Net:     {TotalNet}");
            builder.AppendLine();

            foreach (var m in Months)
            {
                builder.AppendLine(m.ToString());
                builder.AppendLine(new string('-', 40));
            }

            return builder.ToString();
        }
    }
}