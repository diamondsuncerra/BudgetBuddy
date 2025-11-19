using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetBuddy.Domain
{
    public class Logger
    {
        private static readonly object _lock = new();
        public static void ImportReport(string message)
        {
            const string reportPath = "logs/import_reports.txt";
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
                lock (_lock)
                {
                    File.AppendAllText(reportPath,
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
        public static void Info(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[INFO]: {0}", message);
            Console.ResetColor();
        }
        public static void Warn(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[WARNING]: {0}", message);
            Console.ResetColor();
        }
        public static void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ERROR]: {0}", message);
            Console.ResetColor();
        }
        public static void GreenInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[SUCCESS]: {0}", message);
            Console.ResetColor();
        }

    }
}