using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetBuddy.Domain.Extensions
{
    public class DateExtensions
    {
        public static string MonthKey(this DateTime date)
        {
            return date.ToString("yyyy-MM", CultureInfo.InvariantCulture);
        }

        public static string ToMonthAndYear(this string year, int month)
        {
            return $"{year:D4}-{month:D2}";
        }
    }
}