using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBuddy.Domain;

namespace BudgetBuddy.Infrastructure.Log
{
    public sealed class FileLogger : ILogger
    {
        private static readonly object _lock = new();
        private readonly string _reportPath;
        public FileLogger(string reportPath = "logs/import_reports.txt")
        {
            _reportPath = reportPath;
        }
        private void Append(string message)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_reportPath)!);
                lock (_lock)
                {
                    File.AppendAllText(_reportPath,
                        $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"[LOGGER ERROR]: Failed to write import report: {ex.Message}");
                Console.ResetColor();
            }
        }
        public void Info(string message) => Append($"[INFO] {message}");
        public void Warn(string message) => Append($"[WARN] {message}");
        public void Error(string message) => Append($"[ERROR] {message}");
        public void Success(string message) => Append($"[SUCCESS] {message}");
        public void Report(string message) => Append(message);
    }
}