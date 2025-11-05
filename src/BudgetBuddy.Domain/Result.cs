namespace BudgetBuddy.Domain
{
    public sealed class Result<T> // no further inheritance
    {
        public boolean IsSuccess { get; }
        public string Error { get; }
        public T? Value { get; }

        public Result(Boolean isSuccess, string error, T? value)
        {
            IsSuccess = isSuccess;
            Error = error;
            Value = value;
        }

        public static Result<T> Ok(T value)
        {
            new Result<T>(true, string.Empty, value);
        }

        public static Result<T> Ok(string error)
        {
            new Result<T>(false, error, default);
        }

    }
}