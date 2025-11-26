using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BudgetBuddy.App;
using BudgetBuddy.Domain;

namespace BudgetBuddy.Infrastructure.Export
{
    public class CsvExportStrategy : IExportStrategy
    {
        private ILogger _logger;

        public CsvExportStrategy(ILogger logger)
        {
            _logger = logger;
        }
        public async Task<bool> Export(string fileName, IEnumerable<Transaction> data, CancellationToken token, bool overwrite)
        {

            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    _logger.Warn("Export failed: File Name is empty.");
                    return false;
                }

                if (!overwrite && File.Exists(fileName))
                {
                    _logger.Warn("Export failed: Overwriting not permitted but file exists.");
                    return false;
                }

                foreach (var t in data)
                {
                    _logger.Info($"Preparing record {t.Id}...");
                    await Task.Delay(100, token); // 100ms delay per record
                }

                var fullPath = Path.GetFullPath(fileName);
                var dir = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                await using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
                await using var writer = new StreamWriter(fs, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                await writer.WriteLineAsync(Info.HeaderFormat);

                foreach (var t in data)
                {
                    token.ThrowIfCancellationRequested();
                    var line = string.Join(",",
                    t.Id,
                    t.Timestamp.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    t.Payee,
                    t.Amount.ToString(CultureInfo.InvariantCulture),
                    t.Currency,
                    t.Category
                    );
                    await writer.WriteLineAsync(line);
                }
                await writer.FlushAsync();

                return true;
            }
            catch (OperationCanceledException)
            {
                _logger.Info(ExportFailure.CSVCancelled);
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