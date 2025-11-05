using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetBuddy.Domain
{
    public class Transaction
    {
        string Id { get; set; }
        DateTime Timestamp { get; set; }
        string Payee { get; set; }
        decimal Amount { get; set; } // |±1,000,000|.
        string Currency { get; set; }
        string Category { get; set; }
    }
}