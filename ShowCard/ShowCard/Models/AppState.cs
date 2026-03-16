using System.Collections.Generic;

namespace ShowCard.Models;

public class AppState
{
    public List<Card> Suspects { get; set; } = new();
    public List<Card> Weapons { get; set; } = new();
    public List<Card> Locations { get; set; } = new();
    public List<Performer> Performers { get; set; } = new();

    public int RevealDelayMs { get; set; } = 1000;
    public string WallpaperPath { get; set; } = string.Empty;
    public string LastBackImagePath { get; set; } = string.Empty;
}
