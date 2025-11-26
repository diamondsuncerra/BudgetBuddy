using BudgetBuddy.App;
using BudgetBuddy.Domain;
using BudgetBuddy.Domain.Abstractions;

namespace BudgetBuddy.Infrastructure.Export
{
    public class ExportService : IExportService
    { // this decices the strategy

        ILogger _logger;
        public ExportService(ILogger logger)
        {
            _logger = logger;
        }
        public async Task<bool> Export(string fileName, string format, IEnumerable<Transaction> data, CancellationToken token, bool overwrite)
        {
            IExportStrategy strategy = format.ToLowerInvariant() switch
            {
                "csv" => new CsvExportStrategy(_logger),
                "json" => new JsonExportStrategy(_logger),
                _ => throw new NotSupportedException(Warnings.InvalidExportFormat)
            };

            return await strategy.Export(fileName, data, token, overwrite);
            // for solid
        }
    }
}