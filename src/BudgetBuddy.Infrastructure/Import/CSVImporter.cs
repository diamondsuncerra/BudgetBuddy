using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BudgetBuddy.Domain;

namespace BudgetBuddy.Infrastructure.Import
{
    public class CSVImporter
    {
        private readonly IRepository<Transaction, string> _repo;
        private readonly object lockObj = new();

        public CSVImporter(IRepository<Transaction, string> repository)
        {
            _repo = repository;
        }

        public async Task ReadAllFilesAsync(IEnumerable<string> fileNames, CancellationToken token)
        {
            int totalImported = 0;
            int totalDuplicates = 0;
            int totalMalformed = 0;

            await Parallel.ForEachAsync(fileNames, token, async (file, ct) =>
            {
                (int i, int d, int m) = await ReadOneFileAsync(file, ct);
                lock (lockObj)
                {
                    totalDuplicates += d;
                    totalImported += i;
                    totalMalformed += m;
                }
            });

            if (fileNames.Count() != 1)
                Logger.Info($"Total Imported: {totalImported}, Total Duplicates: {totalDuplicates}, Total Malformed: {totalMalformed}");
        }

        public async Task<(int imported, int duplicates, int malformed)> ReadOneFileAsync(string fileName, CancellationToken token)
        {
            try
            {
                if (!File.Exists(fileName))
                {
                    Logger.Warn($"[{fileName}] File doesn't exist");
                    return (0, 0, 0);
                }

                string[] lines = await File.ReadAllLinesAsync(fileName, token);
                if (lines.Length <= 1) return (0, 0, 0);

                var shortName = Path.GetFileName(fileName);
                var dataLines = lines.Skip(1).ToArray();
                int total = dataLines.Length;
                int malformed = 0, imported = 0, duplicates = 0;
                int processed = 0;
                int lastProgress = -1;

                Logger.Info($"Starting import: {shortName}");

                foreach (string line in dataLines)
                {
                    token.ThrowIfCancellationRequested();
                    processed++;

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    Result<Transaction> result = TransactionFactory.TryCreate(line);
                    if (!result.IsSuccess)
                    {
                        malformed++;
                        Logger.ImportReport($"[{shortName}] line {processed} malformed: {result.Error}");
                        continue;
                    }

                    if (_repo.TryAdd(result.Value!))
                        imported++;
                    else
                        duplicates++;

                    // Simulate work so you can test cancellation
                    await Task.Delay(50, token);

                    // progress reporting (every 1%)
                    int progress = processed * 100 / total;
                    if (progress != lastProgress)
                    {
                        lastProgress = progress;
                        Console.CursorLeft = 0;
                        Console.Write($"{shortName}: {progress}%   ");
                    }
                }

                Console.WriteLine(); // new line after progress
                Logger.Info($"[{shortName}] Imported: {imported}, Duplicates: {duplicates}, Malformed: {malformed}");
                Logger.ImportReport($"{shortName} → Imported: {imported}, Duplicates: {duplicates}, Malformed: {malformed}");
                return (imported, duplicates, malformed);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine();
                Logger.Warn($"[{fileName}] Import cancelled by user.");
                Logger.ImportReport($"{fileName} → CANCELLED by user.");
                return (0, 0, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Logger.Error($"[{fileName}] Error: {e.Message}");
                Logger.ImportReport($"{fileName} → ERROR: {e.Message}");
                return (0, 0, 0);
            }
        }
    }
}
