namespace ShowCard.Services;

public interface ILogService
{
    event Action<string>? LogMessage;
    void Info(string message);
    void Warn(string message);
    void Error(string message);
    void Error(string message, Exception ex);

}
