namespace WarGame.Shared
{
    public enum GameResult
    {
        PlayerWins,
        OpponentWins,
        Draw
    }

    public enum ResponseStatus
    {
        Success,
        NetworkError,
        Timeout,
        ServerError
    }

    public enum CompareResult
    {
        PlayerWins,
        OpponentWins,
        Tie
    }
}