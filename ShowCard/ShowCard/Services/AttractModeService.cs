using System;
using System.Windows.Forms;
using ShowCard.Models;
using Timer = System.Windows.Forms.Timer;

namespace ShowCard.Services;

public class AttractModeService : IAttractModeService
{
    private readonly ILogService _log;
    private readonly Timer _timer;
    private Action? _showSequence;
    private Action? _hideSequence;
    private bool _showPhase;

    public bool IsRunning { get; private set; }

    public AttractModeService(ILogService log)
    {
        _log = log;
        _timer = new Timer();
        _timer.Tick += Timer_Tick;
    }

    public void Start(AppState state, Action showSequence, Action hideSequence)
    {
        _showSequence = showSequence;
        _hideSequence = hideSequence;
        _showPhase = true;
        _timer.Interval = state.RevealDelayMs * 4; // rough cycle
        _timer.Start();
        IsRunning = true;
        _log.Info("Attract mode started.");
    }

    public void Stop()
    {
        _timer.Stop();
        IsRunning = false;
        _log.Info("Attract mode stopped.");
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (_showPhase)
            _showSequence?.Invoke();
        else
            _hideSequence?.Invoke();

        _showPhase = !_showPhase;
    }
}
