using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetBuddy.Domain.Extensions
{
    public static class DateExtensions
    {

        public static string MonthKey(this DateTime date)
        {
            return date.ToString("yyyy-MM", CultureInfo.InvariantCulture);
        }

        public static string YearKey(this DateTime date)
        {
            return date.ToString("yyyy", CultureInfo.InvariantCulture);
        }


        public static string ToMonthAndYear(this string year, int month)
        {
            return $"{year:D4}-{month:D2}";
        }
    }
}