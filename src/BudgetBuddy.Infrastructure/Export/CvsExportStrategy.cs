using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBuddy.Domain;

namespace BudgetBuddy.Infrastructure.Export
{
    public class CvsExportStrategy : IExportStrategy
    {

        public  Task<bool> Export(string fileName, IEnumerable<Transaction> data, CancellationToken token, bool overwrite)
        {
            throw new NotImplementedException();
        }
    }
}