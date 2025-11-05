using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetBuddy.Domain
{
    public class Logger
    {
        public static void Info(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[INFO]: {0}", message);
            Console.ResetColor();
        }
        public static void Warn(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[INFO]: {0}", message);
            Console.ResetColor();
        }
        
    }
}