namespace BudgetBuddy.App.Abstractions
{
    public interface IImporter
    {
        Task ReadAllFilesAsync(string [] paths, CancellationToken token);
    }
}