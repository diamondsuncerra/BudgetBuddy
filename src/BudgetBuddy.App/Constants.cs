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
}