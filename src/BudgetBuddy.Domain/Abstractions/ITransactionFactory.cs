namespace BudgetBuddy.Domain.Abstractions
{
    public interface ITransactionFactory
    {
        Result<Transaction> TryCreate(string line);
    }
}