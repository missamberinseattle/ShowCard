using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ShowCard.Models;
using Timer = System.Windows.Forms.Timer;

namespace ShowCard.Services;

public class AttractModeService : IAttractModeService
{
    private readonly ILogService _log;
    private readonly Timer _timer;

    private Action<Card?, Card?, Card?>? _showSequence;
    private Action? _hideSequence;

    private AppState? _state;
    private List<Card> _suspects = new();
    private List<Card> _weapons = new();    
    private List<Card> _locations = new();  

    private bool _showPhase;

    private readonly Random _rng = new();

    // NEW: internal rotation list
    private Queue<Card>? _suspectQueue;

    public bool IsRunning { get; private set; }

    public AttractModeService(ILogService log)
    {
        _log = log;
        _timer = new Timer();
        _timer.Tick += Timer_Tick;
    }

    public void Start(
        AppState state,
        Action<Card?, Card?, Card?> showSequence,
        Action hideSequence)
    {
        _state = state;
        _showSequence = showSequence;
        _hideSequence = hideSequence;

        ShuffleCards(state);

        // Build initial shuffled suspect queue
        _suspectQueue = new Queue<Card>(_suspects);

        _showPhase = true;
        _timer.Interval = state.RevealDelayMs * 4;
        _timer.Start();

        IsRunning = true;
        _log.Info("Attract mode started (non-repeating suspects).");
    }

    private void ShuffleCards(AppState state)
    {
        _suspects = CardRunService.ShuffleList(state.Suspects);
        _weapons = CardRunService.ShuffleList(state.Weapons);
        _locations = CardRunService.ShuffleList(state.Locations);
    }

    public void Stop()
    {
        _timer.Stop();
        IsRunning = false;
        _log.Info("Attract mode stopped.");
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (_state == null || _suspectQueue == null)
            return;

        if (_showPhase)
        {
            // If suspects exhausted, reshuffle
            if (_suspectQueue.Count == 0)
            {
                _log.Info("Suspect list exhausted — reshuffling.");
                ShuffleCards(_state);
                _suspectQueue = new Queue<Card>(_suspects);
            }

            // Pull next suspect
            var suspect = _suspectQueue.Dequeue();
            var index = _suspectQueue.Count(); 

            // Random weapon + location each time
            var weapon = _state.Weapons[index];
            var location = _state.Locations[index];

            _showSequence?.Invoke(suspect, weapon, location);
        }
        else
        {
            _hideSequence?.Invoke();
        }

        _showPhase = !_showPhase;
    }
}
