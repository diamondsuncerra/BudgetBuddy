using BudgetBuddy.Domain.Abstractions;
using BudgetBuddy.Domain;
namespace BudgetBuddy.Infrastructure.Import
{
    public sealed class CsvImportAdapter : IImporter
    {
        private readonly CsvImporter _importer;
        // for the purpose of decoupling App from Infrastructure
        // so that app doesnt depend on infr anymore. 
        // also you can add more import methods later

        public CsvImportAdapter(IRepository<Transaction, string> repository, ILogger logger)
        {
            _importer = new CsvImporter(repository, logger);
        }
        public async Task ReadAllFilesAsync(string[] paths, CancellationToken token)
        {
            await _importer.ReadAllFilesAsync(paths, token);
        }
    }
}