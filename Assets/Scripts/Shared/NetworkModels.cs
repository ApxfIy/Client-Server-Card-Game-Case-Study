namespace WarGame.Shared
{
    public class StartGameResponse
    {
        public string ErrorMessage;
        public bool IsRestoredGame;

        // War state (meaningful when IsWarActive = true)
        public bool IsWarActive;
        public int OpponentCapturedCount;
        public int OpponentHandCount;
        public CardRank[] OpponentSlotRanks = System.Array.Empty<CardRank>();
        public int PlayerCapturedCount;

        public int PlayerHandCount;
        public CardRank[] PlayerSlotRanks = System.Array.Empty<CardRank>();
        public int ReshuffleCount;
        public ResponseStatus Status;
        public int WarFaceDownCount;

        public int TotalCards =>
            PlayerHandCount
            + OpponentHandCount
            + PlayerCapturedCount
            + OpponentCapturedCount
            + WarFaceDownCount
            + (PlayerSlotRanks?.Length ?? 0)
            + (OpponentSlotRanks?.Length ?? 0);
    }

    public class PlayRoundResponse
    {
        // True when game ended before any cards were played (e.g. can't continue war)
        public bool EarlyGameOver;
        public string ErrorMessage;
        public GameResult FinalResult;

        public bool IsGameOver;
        public int OpponentCapturedCount;
        public CardRank OpponentCard;
        public int OpponentHandCount;
        public bool OpponentHandReshuffled;
        public int OpponentWarCardsPlayed;
        public int PlayerCapturedCount;

        // Ranks revealed this round (set only when cards were actually played)
        public CardRank PlayerCard;

        public int PlayerHandCount;

        // Whether captured pile was reshuffled into hand before this round
        public bool PlayerHandReshuffled;

        // How many face-down war cards were moved to the table (0 or 3 per side)
        public int PlayerWarCardsPlayed;

        public CompareResult RoundResult;
        public ResponseStatus Status;
    }
}