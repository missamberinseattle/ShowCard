using ShowCard.Models;

namespace ShowCard.Services;

public interface IAttractModeService
{
    void Start(AppState state, Action<Card?, Card?, Card?> showSequence, Action hideSequence);

    void Stop();
    bool IsRunning { get; }
}
