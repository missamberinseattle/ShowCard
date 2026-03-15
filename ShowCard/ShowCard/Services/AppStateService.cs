using System;
using System.IO;
using System.Text.Json;
using ShowCard.Models;

namespace ShowCard.Services;

public class AppStateService : IAppStateService
{
    private readonly ILogService _log;
    private readonly string _statePath;
    public AppState State { get; private set; } = new();

    public AppStateService(ILogService log)
    {
        _log = log;
        _statePath = Path.Combine(AppContext.BaseDirectory, "ShowCardState.json");
    }

    public void Load()
    {
        try
        {
            if (File.Exists(_statePath))
            {
                var json = File.ReadAllText(_statePath);
                var state = JsonSerializer.Deserialize<AppState>(json);
                if (state != null)
                    State = state;
            }
            _log.Info("State loaded.");
        }
        catch (Exception ex)
        {
            _log.Info($"Error loading state: {ex.Message}");
        }
    }

    public void Save()
    {
        try
        {
            FileBackupHelper.BackupFile(_statePath, _log);
            var json = JsonSerializer.Serialize(State, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_statePath, json);
            _log.Info("State saved.");
        }
        catch (Exception ex)
        {
            _log.Info($"Error saving state: {ex.Message}");
        }
    }
}
