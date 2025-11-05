using System.Data;

namespace BudgetBuddy.App;
public class Program
{
    public static void Main(string[]args)
    {
        Console.WriteLine("Welcome to Budget Buddy!\nPlease choose one of the following:");
        ConsoleHelper.PrintAllOptions();
        
        bool looping = true;
        while(looping)
        {
            string? input = Console.ReadLine();

            if (!Enum.TryParse(input, true, out ConsoleCommands command))
            {
                Console.WriteLine("Unknown command.");
                continue;
            }

            switch(command)
            {
                case ConsoleCommands.Import:
                    Console.Write("Importing.");
                    break;
                case ConsoleCommands.Exit:
                    looping = false;
                    break;
            }
            
        }
    }
}