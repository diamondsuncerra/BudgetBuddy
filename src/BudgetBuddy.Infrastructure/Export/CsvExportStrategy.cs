using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BudgetBuddy.Domain;

namespace BudgetBuddy.Infrastructure.Export
{
    public class CsvExportStrategy : IExportStrategy
    {

        public async Task<bool> Export(string fileName, IEnumerable<Transaction> data, CancellationToken token, bool overwrite)
        {

            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                   // Logger.Warn("Export failed: File Name is empty.");
                    return false;
                }

                if (!overwrite && File.Exists(fileName))
                {
                    //Logger.Warn("Export failed: Overwriting not permitted but file exists.");
                    return false;
                }

                foreach (var t in data)
                {
                  //  Logger.Info($"Preparing record {t.Id}...");
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
                await writer.WriteLineAsync("Id,Timestamp,Payee,Amount,Currency,Category");

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
               // Logger.Info("CSV export cancelled.");
                return false;
            }
            catch (Exception ex)
            {
                //Logger.Warn($"Export failed: {ex.Message}");
                return false;
            }

        }
    }
}