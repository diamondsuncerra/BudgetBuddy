using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBuddy.Domain;

namespace BudgetBuddy.Infrastructure.Log
{
    public sealed class ConsoleLogger : ILogger
    {
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
    }
}