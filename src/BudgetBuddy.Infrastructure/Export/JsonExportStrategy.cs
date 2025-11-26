using System.Globalization;
using System.Text.Json;
using BudgetBuddy.App;
using BudgetBuddy.Domain;

namespace BudgetBuddy.Infrastructure.Export
{
    public class JsonExportStrategy : IExportStrategy
    {
        private ILogger _logger;

        public JsonExportStrategy(ILogger logger)
        {
            _logger = logger;
        }
        private static readonly JsonSerializerOptions s_options = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
        public async Task<bool> Export(string fileName, IEnumerable<Transaction> data, CancellationToken token, bool overwrite)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    _logger.Warn(ExportFailure.FileNameEmpty);
                    return false;
                }

                if (!overwrite && File.Exists(fileName))
                {
                    _logger.Warn(ExportFailure.OverwritingNotPermitted);
                    return false;
                }

                var dir = Path.GetDirectoryName(Path.GetFullPath(fileName));
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                await using var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);

                foreach (var t in data)
                {
                    _logger.Info($"Preparing record {t.Id}...");
                    await Task.Delay(100, token); // 100ms delay per record
                }

                var formatedData = data.Select(t => new
                {
                    t.Id,
                    Timestamp = t.Timestamp.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    t.Payee,
                    t.Amount,
                    t.Currency,
                    t.Category
                });
                await JsonSerializer.SerializeAsync(fs, formatedData, s_options, token);
                await fs.FlushAsync(token);
                return true;
            }
            catch (OperationCanceledException)
            {
                _logger.Info(ExportFailure.JSONCancelled);
                return false;
            }
            catch (Exception ex)
            {
                _logger.Warn($"Export failed: {ex.Message}");
                return false;
            }
        }
    }
}