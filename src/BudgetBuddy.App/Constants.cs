namespace BudgetBuddy.App
{
    public enum ConsoleCommands
    {
        Import,
        ListAll,
        ListMonth,
        ByCategory,
        Over,
        Search,
        SetCategory,
        RenameCategory,
        Remove,
        StatsMonth,
        StatsYearly,
        Export,
        Help,
        Exit
    }

    public enum ExportFormat { Json, Csv }
    public class Warnings
    {
        public const string InvalidCommand = "Invalid command.";
        public const string CommandNotImplemented = "Command not implemented yet.";
        public const string ImproperUsage = "Improver usage of ";
        public const string InvalidMonth = "Invalid month";
    }

    public class Info
    {
        public const string NoTransactionsFound = "No transactions were found";
        public const string OverwritingPrompt = "This file already exists. Do you want to overwrite it? (y/n): ";
        public const string Welcome = "Welcome to Budget Buddy!\nPlease choose one of the following:";
        public const string ExitMessage = "Exiting application. Goodbye!";
        public const string CancellingMessage = "Cancelling... please wait.";
    }
    public class ProperUsage
    {
        public const string Import = "import <file1.csv> [file2.csv...]";
        public const string List = "list all\nlist month<yyyy-MM>";
        public const string ByCategory = "by category<name>";
        public const string Over = "over<amount>";
        public const string Search = "search <text>";
        public const string SetCategory = "set category <id> <name>";
        public const string RenameCategory = "rename category <old> <new>";
        public const string Remove = "remove<id>";
        public const string Stats = "stats month <yyyy-MM>\nstats yearly<yyyy>";
    
        public const string Export = "export json <path>\nexport csv <path>";

        public const string Help = "help";
        public const string Exit= "exit";
        
    }

    public enum HeaderFormat
    {
        Id, Timestamp, Payee, Amount, Currency, Category
    }
}