using System;
using System.IO;
using System.Text;

namespace ShowCard.Services;

public class LogService : ILogService
{
    private readonly string _logFilePath;

    public event Action<string>? LogMessage;

    public LogService()
    {
        var exeDir = AppContext.BaseDirectory;
        _logFilePath = Path.Combine(exeDir, "ShowCard.log");

        if (!File.Exists(_logFilePath))
        {
            try
            {
                File.WriteAllText(_logFilePath, $"ShowCard Log - Created at {DateTime.Now}\n", Encoding.UTF8);
            }
            catch
            {
                // ignore logging failures
            }
        }
    }

    public void Info(string message)
    {
        WriteLog("INFO", message);
    }

    public void Warn(string message)
    {
        WriteLog("WARN", message);
    }

    public void Error(string message)
    {
        WriteLog("ERROR", message);
    }

    public void Error(string message, Exception ex)
    {
        WriteLog("ERROR", $"{message} - Exception: {ex}");
    }

    private void WriteLog(string env, string message)
    {
        var line = $"{DateTime.Now:HH:mm:ss} {env} {message}";
        LogMessage?.Invoke(line);

        try
        {
            File.AppendAllText(_logFilePath, line + Environment.NewLine, Encoding.UTF8);
        }
        catch
        {
            // ignore logging failures
        }

    }

}
