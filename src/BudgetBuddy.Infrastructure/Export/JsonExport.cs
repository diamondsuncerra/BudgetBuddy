using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BudgetBuddy.Domain;

namespace BudgetBuddy.Infrastructure.Export
{
    public class JsonExport : IExportStrategy
    {

        public async static Task Export(string fileName, IDictionary<string, Transaction> data, CancellationToken token)
        {
            await using var fs = File.Create(fileName);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            await JsonSerializer.SerializeAsync(fs, data, options, token);
            await fs.FlushAsync(token);
        }
    }
}