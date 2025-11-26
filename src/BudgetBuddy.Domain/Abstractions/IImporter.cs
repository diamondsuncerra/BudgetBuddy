namespace BudgetBuddy.Domain.Abstractions
{
    public interface IImporter
    {
        Task ReadAllFilesAsync(string [] paths, CancellationToken token);
        // era mai bine in app dar dupa sunt dependinte circulare si il punem aici
        
    }
}