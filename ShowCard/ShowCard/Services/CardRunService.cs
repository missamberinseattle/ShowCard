using System;
using System.Collections.Generic;
using System.Linq;
using ShowCard.Models;

namespace ShowCard.Services;


public class CardRunService : ICardRunService
{
    private readonly ILogService _log;
    private static readonly Random _rng = new();

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

        if (suspects.Count < performers.Count)
        {
            _log.Warn("Not enough suspect cards for all performers. Some performers will share suspects.");
        }

        if (weapons.Count < performers.Count)
        {
            _log.Warn("Not enough weapon cards for all performers. Some performers will share weapons.");
        }

        if (locations.Count < performers.Count)
        {
            _log.Warn("Not enough location cards for all performers. Some performers will share locations.");
        }   

        var shuffledSuspects = ShuffleList(suspects);
        var shuffledWeapons = ShuffleList(weapons);
        var shuffledLocations = ShuffleList(locations);

        int wIndex = 0;

        foreach (var performer in performers.OrderBy(p => p.RunOrder))
        {
            performer.Suspect = shuffledSuspects[wIndex];
            performer.Weapon = shuffledWeapons[wIndex];
            performer.Location = shuffledLocations[wIndex];
            wIndex++;
            
            _log.Info($"Assigned cards to performer: {performer}.");
        }
    }

    public static List<Card> ShuffleList(List<Card> cards)
    {
        var shuffled = cards.ToArray();

        for(var xx = 0; xx < cards.Count; xx++)
        {
            var swapIndex = _rng.Next(cards.Count);
            var temp = shuffled[xx];
            shuffled[xx] = shuffled[swapIndex];
            shuffled[swapIndex] = temp;
        }

        return shuffled.ToList();
    }

    public Performer? GetNextPerformer(List<Performer> performers, int currentRunOrder)
    {
        return performers
            .Where(p => p.RunOrder > currentRunOrder)
            .OrderBy(p => p.RunOrder)
            .FirstOrDefault();
    }
}
