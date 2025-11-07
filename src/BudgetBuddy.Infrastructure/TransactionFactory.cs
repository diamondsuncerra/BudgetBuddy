using BudgetBuddy.Domain;
namespace BudgetBuddy.Infrastructure;

public class TransactionFactory
{
    public Result<Transaction> TryCreate(string line, string[] headerNames, int lineNumber)
    { // validate and return result either fail or ok

        if (string.IsNullOrWhiteSpace(line))
            return Result<Transaction>.Fail("Fail");

        string[] values = line.Split(','); // if fields contain commas there will be problems
        if (values.Length < 5)
            return Result<Transaction>.Fail("Fail");

        string id       = values[0].Trim();
        string timestamp   = values[1].Trim();
        string payee    = values[2].Trim();
        string amtText  = values[3].Trim();
        string currency = values[4].Trim();
        string category = values.Length >= 6 ? values[5].Trim() : string.Empty;

        if (string.IsNullOrWhiteSpace(id)) return Result<Transaction>.Fail("Fail");
        if (string.IsNullOrWhiteSpace(payee)) return Result<Transaction>.Fail("Fail");
        if (string.IsNullOrWhiteSpace(currency)) return Result<Transaction>.Fail("Fail");

        var dateResult = timestamp.TryDate();
        if(!dateResult.IsSuccess) return Result<Transaction>.Fail("Fail");

        var amountResult = amtText.TryDec();
        if (!amountResult.IsSuccess) return Result<Transaction>.Fail("Fail");


        var amount = amountResult.Value!;
        if (amount < -1_000_000m || amount > 1_000_000m)
            return Result<Transaction>.Fail("Fail");

        if (string.IsNullOrWhiteSpace(category))
            category = "Uncategorized";

        var tx = new Transaction
        {
            Id = id,
            Timestamp = dateResult.Value!,
            Payee = payee,
            Amount = amount,
            Currency = currency,
            Category = category
        };

        return Result<Transaction>.Ok(tx);
    }

}
