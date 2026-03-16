using ShowCard.Models;

namespace ShowCard.Services;

public interface IAppStateService
{
    AppState State { get; }
    void Load();
    void Save();
}
