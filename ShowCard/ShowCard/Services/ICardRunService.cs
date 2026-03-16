using ShowCard.Models;

namespace ShowCard.Services
{
    public interface ICardRunService
    {
        void ShuffleAndAssign(List<Performer> performers, List<Card> suspects, List<Card> weapons, List<Card> locations);
        Performer? GetNextPerformer(List<Performer> performers, int currentRunOrder);
    }
}
