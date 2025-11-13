using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBuddy.Domain;

namespace BudgetBuddy.Infrastructure.Export
{
    public class CvsExport : IExportStrategy
    {


        public static void Export(string fileName, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public static Task Export(string fileName, IDictionary<string, Transaction> data, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}