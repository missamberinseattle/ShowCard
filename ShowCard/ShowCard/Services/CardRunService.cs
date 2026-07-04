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


        if (weapons.Count == 0)
        {
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
            performer.Suspect = shuffledSuspects[Math.Min(wIndex, shuffledSuspects.Count - 1)];
            performer.Weapon = shuffledWeapons[Math.Min(wIndex, shuffledWeapons.Count - 1)];
            performer.Location = shuffledLocations[Math.Min(wIndex, shuffledLocations.Count - 1)];

            wIndex++;

            _log.Info($"Assigned cards to performer: {performer}.");
        }

        var validations = FindDuplicateAssignedCards(performers);

        if (validations.Count > 0)
        {
            _log.Warn($"Duplicate cards found: {string.Join(", ", validations)}");
        }
    }

    public static List<Card> ShuffleList(List<Card> cards)
    {
        var shuffled = cards.ToArray();

        for (var xx = 0; xx < cards.Count; xx++)
        {
            var swapIndex = _rng.Next(cards.Count);
            var temp = shuffled[xx];
            shuffled[xx] = shuffled[swapIndex];
            shuffled[swapIndex] = temp;
        }

        return shuffled.ToList();
    }

    public List<string> FindDuplicateAssignedCards(List<Performer> performers)
    {
        // Collect all assigned cards
        var allAssigned = new List<Card>();

        foreach (var p in performers)
        {
            if (p.Suspect != null)
            {
                allAssigned.Add(p.Suspect);
            }
            else
            {
                allAssigned.Add(BuildErrorCard(p.Name, p.Suspect, "Suspect"));
            }
            if (p.Weapon != null)
            {
                allAssigned.Add(p.Weapon);
            }
            else
            {
                allAssigned.Add(BuildErrorCard(p.Name, p.Weapon, "Weapon"));
            }
            if (p.Location != null)
            {
                allAssigned.Add(p.Location);
            }
            else
            {
                allAssigned.Add(BuildErrorCard(p.Name, p.Weapon, "Location"));
            }
        }

        // Group by Title (your only unique identifier)
        var duplicates = allAssigned
            .GroupBy(c => c.Title)
            .Where(g => g.Count() > 1)
            .Select(g => g.First())
            .ToList();

        foreach (var item in allAssigned)
        {
            if (item.Title.EndsWith("null")) duplicates.Add(item);
        }

        return duplicates.Select(c => c.Title).ToList<string>();
    }

    private Card BuildErrorCard(string name, Card? card, string cardType)
    {
        return new Card { Title = $"Performer: {name}; Type: {cardType}; Card: {(card != null ? card.Title : "null")}" };
    }

    public Performer? GetNextPerformer(List<Performer> performers, int currentRunOrder)
    {
        return performers
            .Where(p => p.RunOrder > currentRunOrder)
            .OrderBy(p => p.RunOrder)
            .FirstOrDefault();
    }
}
