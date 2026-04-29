namespace WarGame.Shared
{
    public class StartGameResponse
    {
        public ResponseStatus Status;
        public string ErrorMessage;
        public bool IsRestoredGame;

        public int PlayerHandCount;
        public int OpponentHandCount;
        public int PlayerCapturedCount;
        public int OpponentCapturedCount;

        // War state (meaningful when IsWarActive = true)
        public bool IsWarActive;
        public int WarFaceDownCount;
        public CardRank[] PlayerSlotRanks   = System.Array.Empty<CardRank>();
        public CardRank[] OpponentSlotRanks = System.Array.Empty<CardRank>();
    }

    public class PlayRoundResponse
    {
        public ResponseStatus Status;
        public string ErrorMessage;

        // Ranks revealed this round (set only when cards were actually played)
        public CardRank PlayerCard;
        public CardRank OpponentCard;

        // How many face-down war cards were moved to the table (0 or 3 per side)
        public int PlayerWarCardsPlayed;
        public int OpponentWarCardsPlayed;

        // Whether captured pile was reshuffled into hand before this round
        public bool PlayerHandReshuffled;
        public bool OpponentHandReshuffled;

        public CompareResult RoundResult;

        public int PlayerHandCount;
        public int OpponentHandCount;
        public int PlayerCapturedCount;
        public int OpponentCapturedCount;

        public bool IsGameOver;
        // True when game ended before any cards were played (e.g. can't continue war)
        public bool EarlyGameOver;
        public GameResult FinalResult;
    }
}
