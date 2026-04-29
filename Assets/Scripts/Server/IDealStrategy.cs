using System.Collections.Generic;
using WarGame.Shared;

namespace WarGame.Server
{
    public interface IDealStrategy
    {
        void Deal(List<CardRank> playerHand, List<CardRank> opponentHand);
        void Shuffle(List<CardRank> list, int reshuffleNumber);
    }
}