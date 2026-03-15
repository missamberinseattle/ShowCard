using System;
using System.Collections.Generic;
using System.Linq;
using ShowCard.Models;

namespace ShowCard.Services;


public class CardRunService : ICardRunService
{
    private readonly ILogService _log;
    private readonly Random _rng = new();

    public CardRunService(ILogService log)
    {
        _log = log;
    }

    public void ShuffleAndAssign(List<Performer> performers, List<Card> suspects, List<Card> weapons, List<Card> locations)
    {
        if (performers.Count == 0)
        {
            _log.Warn("No performers available to assign cards.");
            return;
        }

        if (suspects.Count == 0)
        {
            _log.Warn("No suspect cards available for assignment.");
            return;
        }


        if (weapons.Count == 0) {
            _log.Warn("No weapon cards available for assignment.");
            return;
        }

        if (locations.Count == 0)
        {
            _log.Warn("No location cards available for assignment.");
            return;
        }

        var shuffledSuspects = suspects.OrderBy(_ => _rng.Next()).ToList();
        var shuffledWeapons = weapons.OrderBy(_ => _rng.Next()).ToList();
        var shuffledLocations = locations.OrderBy(_ => _rng.Next()).ToList();

        int wIndex = 0, lIndex = 0;

        foreach (var performer in performers.OrderBy(p => p.RunOrder))
        {
            performer.Suspect = shuffledSuspects[_rng.Next(shuffledSuspects.Count)];
            performer.Weapon = shuffledWeapons[wIndex % shuffledWeapons.Count];
            performer.Location = shuffledLocations[lIndex % shuffledLocations.Count];
            wIndex++;
            lIndex++;
            _log.Info($"Assigned cards to performer {performer.Name}.");
        }
    }

    public Performer? GetNextPerformer(List<Performer> performers, int currentRunOrder)
    {
        return performers
            .Where(p => p.RunOrder > currentRunOrder)
            .OrderBy(p => p.RunOrder)
            .FirstOrDefault();
    }
}
