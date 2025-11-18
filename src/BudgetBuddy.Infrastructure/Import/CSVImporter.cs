using System;
using System.Collections.Generic;
using System.Linq;
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


            await Parallel.ForEachAsync(fileNames, token, async (file, token) =>
            {
                (int i, int d, int m) = await ReadOneFileAsync(file, token);
                lock (lockObj)
                {
                    totalDuplicates += d;
                    totalImported += i;
                    totalMalformed += m;
                }
            });

            if(fileNames.Count() != 1) 
                Logger.Info($"Total Imported: {totalImported}, Total Duplicates: {totalDuplicates}, Total Malformed: {totalMalformed}");

        }
        public async Task<(int imported, int duplicates, int malformed)> ReadOneFileAsync(string fileName, CancellationToken token)
        {

            try
            {
                if (File.Exists(fileName))
                {
                    string[] lines = await File.ReadAllLinesAsync(fileName, token);
                    if (lines.Length <= 1) return (0, 0, 0);
                    var header = lines[0];
                    var dataLines = lines.Skip(1);
                    int malformed = 0;
                    int imported = 0;
                    int duplicates = 0;
                    int lineNumber = 0;
                    foreach (string line in dataLines)
                    {
                        token.ThrowIfCancellationRequested();
                        lineNumber++;
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        Result<Transaction> result = TransactionFactory.TryCreate(line);

                        if (!result.IsSuccess)
                        {
                            malformed++;
                            Logger.Warn($"The line {lineNumber} was malformed.");
                            Logger.Warn($"The error was: {result.Error}");
                            continue;
                        }

                        if (_repo.TryAdd(result.Value!))
                        {
                            imported++;
                            Logger.Info($"The line {lineNumber} was registered safely.");
                        }
                        else
                        {
                            duplicates++;
                            Logger.Warn($"The line {lineNumber} was duplicate.");
                        }

                    }
                    Logger.Info($"Imported: {imported}, Duplicates: {duplicates}, Malformed: {malformed}");
                    return (imported, duplicates, malformed);
                }
                else
                {
                    Logger.Warn("File doesn't exist");
                    return (0, 0, 0);
                }
            }
            catch (OperationCanceledException)
            {
                Logger.Info($"[{fileName}] Import cancelled.");
                return (0, 0, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                return (0, 0, 0);
            }
        }
    }
}