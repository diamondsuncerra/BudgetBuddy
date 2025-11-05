using BudgetBuddy.Domain;
namespace BudgetBuddy.Infrastructure;

public class TransactionFactory
{

    public Result<Transaction> TryCreate(string line, string[] headerNames, int lineNumber)
    { // validate and return result either fail or ok

        string[] values = line.Split(','); // if fields contain commas there will be problems
        string id = values[0];
        if (id == null) return Result<Transaction>.Fail("Id is null.");
        //etc

    }

}
