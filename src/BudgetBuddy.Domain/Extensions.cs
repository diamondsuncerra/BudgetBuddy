using System.Globalization;
namespace BudgetBuddy.Domain;

public static class Extensions
{
    public static string ToMoney(this decimal value, string currency)
    {
        return string.Format(CultureInfo.InvariantCulture, "{0:N2} {1}", value, currency);
    }

    public static decimal SumAbs(this decimal a, decimal b)
    {
        return Math.Abs(a) + Math.Abs(b);
    }
    public static decimal SumAbs(this IEnumerable<decimal> arr)
    {
        return arr.Sum(x => Math.Abs(x));
    }
    public static decimal AverageAbs(this decimal a, decimal b)
    {
        return (Math.Abs(a) + Math.Abs(b)) / 2;
    }
    public static decimal AverageAbs(this IEnumerable<decimal> arr)
    {
        ArgumentNullException.ThrowIfNull(arr);
        return arr.Average(x => Math.Abs(x));
    }

    public static string MonthKey(this DateTime date)
    {
        return date.ToString("yyyy-MM", CultureInfo.InvariantCulture);
    }

    public static Result<decimal> TryDec(this string text)
    {
        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
        {
            return Result<decimal>.Ok(value);
        }
        else
        {
            return Result<decimal>.Fail($"Invalid decimal: '{text}'");
        }
    } 
    
        public static Result<DateTime> TryDate(this string text)
    {
        if(DateTime.TryParseExact(text,"yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return Result<DateTime>.Ok(date);
        } else
        {
            return Result<DateTime>.Fail($"Invalid date: '{text}'");
        }
    } 
}
