using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetBuddy.App
{
    public class ConsoleHelper
    {
        public static void PrintAllOptions()
        {
            Console.WriteLine("Import");
            Console.WriteLine("List all");
            Console.WriteLine("List month <yyyy-MM>");
            Console.WriteLine("By category <name>");
            Console.WriteLine("Over <amount>");
            Console.WriteLine("Search <text>");
            Console.WriteLine("Set category <id> <name>");
            Console.WriteLine("Rename category <old> <new>");
            Console.WriteLine("Remove <id>");
            Console.WriteLine("Stats month <yyyy-MM>");
            Console.WriteLine("Stats yearly <yyyy>");
            Console.WriteLine("Export json <path>");
            Console.WriteLine("Export cvs <path>"); // prompt for overwritting
            Console.WriteLine("Help");
            Console.WriteLine("Exit");
        }
    }
}