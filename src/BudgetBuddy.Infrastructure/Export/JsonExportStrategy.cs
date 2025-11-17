using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using BudgetBuddy.Domain;

using Microsoft.VisualBasic;

namespace BudgetBuddy.Infrastructure.Export
{
    public class JsonExportStrategy : IExportStrategy
    {

        public async  Task<bool> Export(string fileName, IEnumerable<Transaction> data, CancellationToken token, bool overwrite)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                Logger.Warn("Export failed: File Name is empty.");
                return false;
            }

            if (!overwrite && File.Exists(fileName))
            {
                Logger.Warn("Export failed: Overwriting not permitted but file exists.");
                return false;
            }

            var dir = Path.GetDirectoryName(Path.GetFullPath(fileName));
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            await using var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            await JsonSerializer.SerializeAsync(fs, data, options, token);
            await fs.FlushAsync(token);
            return  true;
        }
    }
}