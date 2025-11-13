using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace BudgetBuddy.Domain
{
    public interface IExportStrategy
    {
        public static abstract Task Export(string fileName, IDictionary<string, Transaction> data, CancellationToken token);
    }
}