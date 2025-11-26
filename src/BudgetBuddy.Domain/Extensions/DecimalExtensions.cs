using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetBuddy.Domain.Extensions
{
    public static class DecimalExtensions
    {
        public static decimal SumAbs(this IEnumerable<decimal> arr)
        {
            return arr.Sum(x => Math.Abs(x));
        }
        public static decimal AverageAbs(this IEnumerable<decimal> arr)
        {
            ArgumentNullException.ThrowIfNull(arr);
            return Math.Round(arr.Average(x => Math.Abs(x)),2);
        }

    }
}