using System;
using WarGame.Shared;

namespace WarGame.Server
{
    [Serializable]
    internal class SavedGameState
    {
        public CardRank[] PlayerHand        = Array.Empty<CardRank>();
        public CardRank[] OpponentHand      = Array.Empty<CardRank>();
        public CardRank[] PlayerCaptured    = Array.Empty<CardRank>();
        public CardRank[] OpponentCaptured  = Array.Empty<CardRank>();
        public bool IsWarActive;
        public CardRank[] WarFaceDown       = Array.Empty<CardRank>();
        public CardRank[] PlayerSlotRanks   = Array.Empty<CardRank>();
        public CardRank[] OpponentSlotRanks = Array.Empty<CardRank>();
        public bool IsGameOver;
        public int ReshuffleCount;
    }
}
