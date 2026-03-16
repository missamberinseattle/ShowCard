using ShowCard.Models;

namespace ShowCard.Models;

public class Performer
{
    public int RunOrder { get; set; }
    public string Name { get; set; } = string.Empty;
    public Card? Suspect { get; set; }
    public Card? Weapon { get; set; }
    public Card? Location { get; set; }

    public override string ToString() => $"{RunOrder}: {Name} [{Suspect}; {Weapon}; {Location}]";
}
