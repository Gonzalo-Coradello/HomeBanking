using HomeBanking.Models;
using System.Collections.Generic;

namespace HomeBanking.Repositories
{
    public interface ICardRepository
    {
        IEnumerable<Card> FindAllCards();
        Card FindById(long id);
        IEnumerable<Card> GetCardsByClient(long clientId);
        void Save(Card card);
    }
}
