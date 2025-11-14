using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBuddy.Domain;

namespace BudgetBuddy.Infrastructure.Export
{
    public class Exporter
    {
        private readonly IExportStrategy _strategy;

        public Exporter(IExportStrategy strategy)
        {
            _strategy = strategy;
        }

        public async Task<bool> Run(string fileName, IEnumerable<Transaction> transactions, CancellationToken token, bool overwrite) 
        {
            return await _strategy.Export(fileName, transactions, token, overwrite );
        }
    }
}