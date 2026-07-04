using ShowCard.Models;

namespace ShowCard.Services;

public interface IAppStateService
{
    AppState State { get; }
    void Load();
    void Load(string path);
    void Save();
    void Save(string path);
}
