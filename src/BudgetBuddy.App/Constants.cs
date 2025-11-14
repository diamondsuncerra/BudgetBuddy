namespace BudgetBuddy.App
{
    public enum ConsoleCommands
    {
        Import,
        List,
        ByCategory,
        Over,
        Search,
        SetCategory,
        RenameCategory,
        Remove,
        Stats,
        Export,
        Help,
        Exit
    }
    public enum ListScope { All, Month }
    public enum StastsScope { Month, Year }
    public enum ExportFormat { Json, Csv }
    public class Warnings
    {
        public static string INVALID_COMMAND = "Invalid command.";
    }
    public class Info
    {
        public static string NO_TRANSACTIONS_FOUND = "No transactions were found";
        public static string OVERWRITING_PROMPT = "This file already exists. Do you want to overwrite it? (y/n): ";
    }
    public enum HeaderFormat
    {
        Id, Timestamp, Payee, Amount, Currency, Category
    }
}