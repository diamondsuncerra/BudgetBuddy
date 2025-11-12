using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBuddy.Domain;
using BudgetBuddy.Infrastructure.Import;

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

        public static bool GetCommand(out ConsoleCommands command, out string[] args)
        {
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                command = default;
                args = Array.Empty<string>();
                return false;
            }
            
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if(parts.Length == 0)
            {
                command = default;
                args = Array.Empty<string>();
                return false;
            }
            var cmdText = parts[0];
            args = parts.Skip(1).ToArray();

            if (!Enum.TryParse(cmdText, true, out command))
            {
                Console.WriteLine("Unknown command.");
                return false;
            }
            return true;
        }
        public static async Task Import(string[]? argText, IRepository<Transaction, string> repo, CancellationToken token)
        {
            CSVImporter importer = new(repo);
            if (argText?.Length < 1)
            {
                Logger.Warn("Improper usage of import.");
            }
            try
            {
                await importer.ReadAllFilesAsync(argText!, token);
            }
            catch (OperationCanceledException)
            {
                Logger.Info("Operation Canceled.");
            }
        }

        public static void List(string[]? argText, IRepository<Transaction, string> repo)
        {
            if (argText?.Length < 1)
            {
                Logger.Warn("Improper usage of list.");
            }

            if (!Enum.TryParse<ListScope>(argText![0], ignoreCase: true, out var scope))
            {
                Logger.Warn("Improper usage of list.");
            }

            switch (scope)
            {
                case ListScope.All:
                    {
                        Console.WriteLine("Listing all records.");
                        var all = repo.GetAll();
                        foreach (var t in all)
                        {
                            Console.WriteLine(t.ToString());
                        }
                        break;
                    }
            }
        }
    }
}