using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetBuddy.Domain.Abstractions
{
    public interface IExportService
    {
         public  abstract Task<bool> Export(string fileName, string format, IEnumerable<Transaction> data, CancellationToken token, bool overwrite);
    }
}