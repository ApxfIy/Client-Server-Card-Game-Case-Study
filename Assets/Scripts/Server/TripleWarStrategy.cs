using System.Collections.Generic;
using WarGame.Shared;

namespace WarGame.Server
{
    // Preset: TripleWar
    // 13 cards each. TakeTop pulls from the end of the list, so the rightmost card is drawn first.
    //
    // Draw sequence (both sides mirror each other until the final comparison):
    //   R1  comparison : Five  vs Five  → Tie
    //   W1  FD × 3     : Three × 3 each
    //   W1  comparison : Six   vs Six   → Tie
    //   W2  FD × 3     : Three × 3 each
    //   W2  comparison : Seven vs Seven → Tie
    //   W3  FD × 3     : Three × 3 each
    //   W3  comparison : Ace   vs Two   → Player wins all 26 cards → game over
    //
    // No player ever wins cards before the final round, so captured piles stay empty
    // and reshuffle cannot occur — Shuffle is a no-op.
    internal class TripleWarStrategy : IDealStrategy
    {
        public void Deal(List<CardRank> playerHand, List<CardRank> opponentHand)
        {
            playerHand.AddRange(new[]
            {
                CardRank.Ace,                                       // W3 comparison (drawn 13th)
                CardRank.Three, CardRank.Three, CardRank.Three,     // W3 FD
                CardRank.Seven,                                     // W2 comparison
                CardRank.Three, CardRank.Three, CardRank.Three,     // W2 FD
                CardRank.Six,                                       // W1 comparison
                CardRank.Three, CardRank.Three, CardRank.Three,     // W1 FD
                CardRank.Five                                       // R1 comparison (drawn 1st)
            });

            opponentHand.AddRange(new[]
            {
                CardRank.Two,                                       // W3 comparison — loses to Ace
                CardRank.Three, CardRank.Three, CardRank.Three,
                CardRank.Seven,
                CardRank.Three, CardRank.Three, CardRank.Three,
                CardRank.Six,
                CardRank.Three, CardRank.Three, CardRank.Three,
                CardRank.Five
            });
        }

        public void Shuffle(List<CardRank> list, int reshuffleNumber) { }
    }
}
