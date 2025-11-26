using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace BudgetBuddy.Domain
{
    public interface IExportStrategy
    {
        public  abstract Task<bool> Export(string fileName, IEnumerable<Transaction> data, CancellationToken token, bool overwrite);
    }
}