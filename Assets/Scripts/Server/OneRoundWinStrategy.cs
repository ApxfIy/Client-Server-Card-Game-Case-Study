using System.Collections.Generic;
using WarGame.Shared;

namespace WarGame.Server
{
    // Presets: PlayerWinsRound1, OpponentWinsRound1
    // Winner gets 51 cards with Ace on top; loser gets a single Two.
    // Game ends after the first comparison — no reshuffle can occur.
    internal class OneRoundWinStrategy : IDealStrategy
    {
        private const int HandCardCount = 4;
        private readonly bool _playerWins;

        public OneRoundWinStrategy(bool playerWins)
        {
            _playerWins = playerWins;
        }

        public void Deal(List<CardRank> playerHand, List<CardRank> opponentHand)
        {
            var strongHand = new List<CardRank>();

            for (var i = 0; i < HandCardCount; i++)
                strongHand.Add(CardRank.Ace);

            var weakHand = new List<CardRank>();

            for (var i = 0; i < HandCardCount; i++)
                weakHand.Add(CardRank.Two);

            if (_playerWins)
            {
                playerHand.AddRange(strongHand);
                opponentHand.AddRange(weakHand);
            }
            else
            {
                playerHand.AddRange(weakHand);
                opponentHand.AddRange(strongHand);
            }
        }

        public void Shuffle(List<CardRank> list, int reshuffleNumber)
        {
        }
    }
}