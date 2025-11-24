using BudgetBuddy.Domain;
using BudgetBuddy.Domain.Extensions;
namespace BudgetBuddy.Infrastructure
{
    public class TransactionFactory
    {
        private const decimal MinAmount = -1_000_000m;
        private const decimal MaxAmount = 1_000_000m;

        public static Result<Transaction> TryCreate(string line)
        {

            if (string.IsNullOrWhiteSpace(line))
                return Result<Transaction>.Fail("Empty line.");

            char delimiter = (line.Contains(';') && !line.Contains(',')) ? ';' : ',';

            string[] values = line.Split(delimiter);
            if (values.Length < 5)
                return Result<Transaction>.Fail(
                    $"Wrong column count: got {values.Length}, expected at least 5 (Id,Timestamp,Payee,Amount,Currency[,Category]). " +
                    $"Detected delimiter '{delimiter}'. Raw: {line}");
            if (values.Length > 6)
                return Result<Transaction>.Fail(
                    $"Wrong column count: got {values.Length}, expected no more than 6 (Id,Timestamp,Payee,Amount,Currency[,Category]). " +
                    $"Detected delimiter '{delimiter}'. Raw: {line}");

            string id = values[0].Trim();
            string tsText = values[1].Trim();
            string payee = values[2].Trim();
            string amtText = values[3].Trim();
            string currency = values[4].Trim();
            string category = values.Length >= 6 ? values[5].Trim() : string.Empty;

            if (string.IsNullOrWhiteSpace(id))
                return Result<Transaction>.Fail("Missing Id.");
            if (string.IsNullOrWhiteSpace(payee))
                return Result<Transaction>.Fail("Missing Payee.");
            if (string.IsNullOrWhiteSpace(currency))
                return Result<Transaction>.Fail("Missing Currency.");

            var dateResult = tsText.TryDate();
            if (!dateResult.IsSuccess)
                return Result<Transaction>.Fail(
                    $"Invalid Timestamp '{tsText}'. Expected format yyyy-MM-dd.");

            if (amtText.Contains(',') && !amtText.Contains('.'))
            {

                return Result<Transaction>.Fail(
                    $"Invalid Amount '{amtText}'. Use dot as decimal separator (InvariantCulture), e.g., 1234.56");
            }

            var amountResult = amtText.TryDec();
            if (!amountResult.IsSuccess)
                return Result<Transaction>.Fail(
                    $"Invalid Amount '{amtText}'. Expected a number like 1234.56 (InvariantCulture).");

            var amount = amountResult.Value!;
            if (amount < MinAmount || amount > MaxAmount)
                return Result<Transaction>.Fail(
                    $"Amount out of range: {amount}. Allowed range is [{MinAmount}, {MaxAmount}].");

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
}
