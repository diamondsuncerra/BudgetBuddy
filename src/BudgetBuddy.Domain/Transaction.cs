using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetBuddy.Domain
{
    public class Transaction
    {
        public required string Id { get; init; }
        public required DateTime Timestamp { get; init; }
        public required string Payee { get; init; }
        public required decimal Amount { get; init; } // |±1,000,000|.
        public required string Currency { get; init; }
        public required string Category { get; set; } 
        // NOTE: diff between init and set, init -> allows assignment only during obj creation
    }
}