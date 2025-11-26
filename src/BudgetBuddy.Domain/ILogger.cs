namespace BudgetBuddy.Domain
{
    public interface ILogger
    {
        void Info(string message);
        void Warn(string message);
        void Error(string message);
        void Success(string message);
        void Report(string message);
        void Log(string message);
    }
}