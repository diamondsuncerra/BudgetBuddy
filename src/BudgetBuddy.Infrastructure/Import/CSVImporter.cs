using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBuddy.Domain;

namespace BudgetBuddy.Infrastructure.Import
{
    public class CSVImporter
    {
        private readonly TransactionsRepository _repo = new();
        public async Task ReadAllFilesAsync(IEnumerable<string> fileNames)
        {
            int totalImported = 0;
            int totalDuplicates = 0;
            int totalMalformed = 0;

            
        }
        public async Task<(int imported, int duplicates, int malformed)> ReadOneFileAsync(string fileName)
        {

            try
            {
                if (File.Exists(fileName))
                {
                    string[] lines = await File.ReadAllLinesAsync(fileName);
                    if (lines.Length <= 1) return(0,0,0);
                    var header = lines[0];
                    var dataLines = lines.Skip(1);
                    int malformed = 0;
                    int imported = 0;
                    int duplicates = 0;
                    int lineNumber = 0;
                    foreach (string line in dataLines)
                    {
                        lineNumber++;
                        Result<Transaction> result = TransactionFactory.TryCreate(line);
                        if (!result.IsSuccess)
                        {
                            malformed++;
                            Logger.Warn($"The line {lineNumber} was malformed.");
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
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                return (0, 0, 0);
            }
        }
    }
}