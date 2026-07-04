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
        Load(_statePath);
    }

    public void Load(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var state = JsonSerializer.Deserialize<AppState>(json);
                if (state != null)
                    State = state;
            }
            _log.Info($"State loaded from {path}.");
        }
        catch (Exception ex)
        {
            _log.Info($"Error loading state: {ex.Message}");
        }
    }

    public void Save()
    {
        Save(_statePath);
    }

    public void Save(string path)
    {
        try
        {
            FileBackupHelper.BackupFile(path!, _log);
            var json = JsonSerializer.Serialize(State, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path!, json);
            _log.Info("State saved.");
        }
        catch (Exception ex)
        {
            _log.Info($"Error saving state: {ex.Message}");
        }
    }

    public static string GetDropboxPath()
    {
        var dropboxPath = Path.Combine(
            new[] { 
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Dropbox",
                "Shows",
                "Murder! By Pasties"
            });

        return dropboxPath;
    }
}
