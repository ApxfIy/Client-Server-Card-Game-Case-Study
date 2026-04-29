using System.Collections.Generic;
using WarGame.Shared;

namespace WarGame.Server
{
    // Preset: WarInsufficientCards
    // Player: 26 cards. Opponent: 7 cards.
    //
    // Draw sequence:
    //   R1  : Ace  vs Two   → Player wins (+2 to captured)
    //   R2  : Five vs Five  → Tie  (_isWarActive = true)
    //   W1  FD × 3: Three each / Four each
    //   W1  comparison: Seven vs Seven → Tie; opponent hand now = 1 card
    //   W2  war check: oTotal = 1 < 4 → EarlyGameOver → PlayerWins
    //
    // Opponent never captures cards, so their captured pile stays empty.
    // Player captures 2 cards in R1 but still has 24 in hand — never runs low.
    // Reshuffle cannot occur — Shuffle is a no-op.
    internal class WarInsufficientCardsStrategy : IDealStrategy
    {
        public void Deal(List<CardRank> playerHand, List<CardRank> opponentHand)
        {
            // Player: 20 filler Threes at the bottom, then the scripted sequence at the top.
            // Drawn order (TakeTop = last element first):
            //   Ace(R1), Five(R2), Three×3(W1 FD), Seven(W1 compare), Three×20(unused filler)
            for (int i = 0; i < 20; i++) 
                playerHand.Add(CardRank.Three);

            playerHand.AddRange(new[]
            {
                CardRank.Seven,                                 // W1 comparison (drawn 6th)
                CardRank.Three, CardRank.Three, CardRank.Three, // W1 FD (drawn 3rd–5th)
                CardRank.Five,                                  // R2 comparison (drawn 2nd)
                CardRank.Ace                                    // R1 comparison (drawn 1st)
            });

            // Opponent: exactly 7 cards.
            // Drawn order: Two(R1), Five(R2), Four×3(W1 FD), Seven(W1 compare), Three(leftover)
            opponentHand.AddRange(new[]
            {
                CardRank.Three,                                 // leftover — 1 card after W1
                CardRank.Seven,                                 // W1 comparison (drawn 6th)
                CardRank.Four, CardRank.Four, CardRank.Four,   // W1 FD (drawn 3rd–5th)
                CardRank.Five,                                  // R2 comparison (drawn 2nd)
                CardRank.Two                                    // R1 comparison (drawn 1st)
            });
        }

        public void Shuffle(List<CardRank> list, int reshuffleNumber) { }
    }
}
