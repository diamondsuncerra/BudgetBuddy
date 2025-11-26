using BudgetBuddy.App;
using BudgetBuddy.Domain;

namespace BudgetBuddy.Infrastructure.Log
{
    public sealed class ConsoleLogger : ILogger
    {
        private string _reportPath = Files.ImportReportFile;
        private static readonly object _lock = new();
        public void Info(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[INFO]: {0}", message);
            Console.ResetColor();
        }
        public void Warn(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[WARNING]: {0}", message);
            Console.ResetColor();
        }
        public void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ERROR]: {0}", message);
            Console.ResetColor();
        }
        public void Success(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[SUCCESS]: {0}", message);
            Console.ResetColor();
        }
        public void Report(string message)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("[SUCCESS]: {0}", message);
            Console.ResetColor();
        }

        public void Log(string message)
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

    }
}