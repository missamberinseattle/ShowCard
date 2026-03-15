namespace ShowCard.Models;

public class Card
{
    public string Title { get; set; } = string.Empty;
    public string FaceImagePath { get; set; } = string.Empty;
    public string BackImagePath { get; set; } = string.Empty;

    public override string ToString() => Title;
}
