using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetBuddy.Domain.Extensions
{
    public class DecimalExtensions
    {
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

    }
}