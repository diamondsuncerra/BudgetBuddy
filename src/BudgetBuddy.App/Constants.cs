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
        Stats,
        Export,
        Help,
        Exit
    }
    public enum StastsScope { Month, Year } 
    public enum ExportFormat {Json, Csv}
}