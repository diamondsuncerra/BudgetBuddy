using BudgetBuddy.Domain;
using BudgetBuddy.Domain.Abstractions;

namespace BudgetBuddy.Infrastructure.Export
{
    public class ExportService : IExportService
    { // this decices the strategy
        public async Task<bool> Export(string fileName, string format, IEnumerable<Transaction> data, CancellationToken token, bool overwrite)
        {
            IExportStrategy strategy;
            if(format.ToLowerInvariant().Equals("csv"))
            {
                strategy = new CsvExportStrategy();
            } else 
            { // n-ar trb sa ajunga pana aici altcv inafara de csv si json
                strategy = new JsonExportStrategy (); 
            } 

            var exporter = new Exporter(strategy);
            return await exporter.Run(fileName, data, token, overwrite);
            // sau as putea direct strategy.Export()
            // cum e mai bine ??
            // ps m-am complicat asa sa fie SOLID!
        }
    }
}