using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetBuddy.Domain.Extensions
{
    public static class ParsingExtentions
    {
          public static Result<DateTime> TryDate(this string text)
    {
        if (DateTime.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return Result<DateTime>.Ok(date);
        }
        else
        {
            return Result<DateTime>.Fail($"Invalid date: '{text}'");
        }
    }

        public static Result<DateTime> TryYear(this string text)
    {
        if (DateTime.TryParseExact(text, "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return Result<DateTime>.Ok(date);
        }
        else
        {
            return Result<DateTime>.Fail($"Invalid date: '{text}'");
        }
    }

    public static Result<DateTime> TryMonth(this string text)
    {
        if (DateTime.TryParseExact(text, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return Result<DateTime>.Ok(date);
        }
        else
        {
            return Result<DateTime>.Fail($"Invalid date: '{text}'");
        }
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

    }
}