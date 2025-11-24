using System.Globalization;
using System.Text;
namespace BudgetBuddy.Domain.Extensions;

public static class FormattingExtensions
{
    public static string ToPrettyTable(this IEnumerable<(string Category, decimal Value)> categories, string currency)
    {
        if (categories == null || !categories.Any())
            return "No categories found.";

        var sb = new StringBuilder();
        sb.AppendLine("Top Expense Categories:");
        sb.AppendLine(new string('-', 33));

        // find the longest category for column alignment
        int maxCatLength = categories.Max(x => x.Category.Length);
        string format = "{0,-" + (maxCatLength + 2) + "}{1,10} {2}";

        foreach (var (category, value) in categories)
        {
            sb.AppendLine(string.Format(format, category, value.ToMoney(currency), ""));
        }

        sb.AppendLine(new string('-', 33));
        return sb.ToString();
    }
    public static string ToMoney(this decimal value, string currency)
    {
        return string.Format(CultureInfo.InvariantCulture, "{0:N2} {1}", value, currency);
    }


}
