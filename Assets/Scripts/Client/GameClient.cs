using System;
using Cysharp.Threading.Tasks;
using WarGame.Server;
using WarGame.Shared;

namespace WarGame.Client
{
    public class GameClient
    {
        private readonly FakeWarServer _server;
        private const int MaxRetries   = 3;
        private const int RetryDelayMs = 600;

        public event Action<string> OnNetworkStatusChanged;

        public GameClient(FakeWarServer server) => _server = server;

        public UniTask<StartGameResponse> StartGameAsync() =>
            ExecuteWithRetry("StartGame", _server.StartGameAsync, LogStartGameResponse,
                (status, msg) => new StartGameResponse { Status = status, ErrorMessage = msg });

        public UniTask<PlayRoundResponse> PlayRoundAsync() =>
            ExecuteWithRetry("PlayRound", _server.PlayRoundAsync, LogPlayRoundResponse,
                (status, msg) => new PlayRoundResponse { Status = status, ErrorMessage = msg });

        private async UniTask<T> ExecuteWithRetry<T>(
            string tag,
            Func<UniTask<T>> action,
            Action<T> onSuccess,
            Func<ResponseStatus, string, T> createError)
        {
            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                GameLogger.Client(attempt == 0
                    ? $"→ {tag}"
                    : $"→ {tag} retry ({attempt}/{MaxRetries - 1})");

                if (attempt > 0)
                    OnNetworkStatusChanged?.Invoke($"Retrying... ({attempt}/{MaxRetries - 1})");

                try
                {
                    var response = await action();
                    if (attempt > 0) OnNetworkStatusChanged?.Invoke(string.Empty);
                    onSuccess(response);
                    return response;
                }
                catch (TimeoutException ex)
                {
                    GameLogger.Client($"← {tag}: TIMEOUT — {ex.Message}");
                    return createError(ResponseStatus.Timeout, ex.Message);
                }
                catch (Exception ex)
                {
                    bool isLast = attempt == MaxRetries - 1;
                    GameLogger.Client(isLast
                        ? $"← {tag}: NETWORK ERROR — giving up after {MaxRetries} attempts ({ex.Message})"
                        : $"← {tag}: network error (attempt {attempt + 1}/{MaxRetries}), retrying in {RetryDelayMs}ms...");

                    if (isLast)
                        return createError(ResponseStatus.NetworkError, "Connection failed");

                    await UniTask.Delay(RetryDelayMs);
                }
            }

            GameLogger.Client($"← {tag}: FAILED — max retries exceeded");
            return createError(ResponseStatus.NetworkError, "Max retries exceeded");
        }

        private static void LogStartGameResponse(StartGameResponse response)
        {
            string kind = response.IsRestoredGame ? "restored" : "new game";
            string war  = response.IsWarActive
                ? $" | war active (pot: {response.WarFaceDownCount}, slots: {response.PlayerSlotRanks?.Length ?? 0} each)"
                : "";
            GameLogger.Client(
                $"← StartGame [{kind}]: " +
                $"Player hand: {response.PlayerHandCount} captured: {response.PlayerCapturedCount} | " +
                $"Opp hand: {response.OpponentHandCount} captured: {response.OpponentCapturedCount}{war}");
        }

        private static void LogPlayRoundResponse(PlayRoundResponse response)
        {
            if (response.EarlyGameOver)
            {
                GameLogger.Client($"← PlayRound: GAME OVER (early, no cards played) — {response.FinalResult}");
                return;
            }

            string war      = response.PlayerWarCardsPlayed > 0 ? $" | war: +{response.PlayerWarCardsPlayed} face-down each" : "";
            string reshuffle = BuildReshuffleNote(response.PlayerHandReshuffled, response.OpponentHandReshuffled);
            string counts   = $"Player hand: {response.PlayerHandCount} captured: {response.PlayerCapturedCount} | " +
                              $"Opp hand: {response.OpponentHandCount} captured: {response.OpponentCapturedCount}";
            string gameOver = response.IsGameOver ? $" | GAME OVER — {response.FinalResult}" : "";

            GameLogger.Client(
                $"← PlayRound [{response.RoundResult}]: " +
                $"Player {response.PlayerCard.RankToString()} vs Opp {response.OpponentCard.RankToString()}" +
                $"{war}{reshuffle} | {counts}{gameOver}");
        }

        private static string BuildReshuffleNote(bool player, bool opponent)
        {
            if (player && opponent) return " | both reshuffled";
            if (player)             return " | Player reshuffled";
            if (opponent)           return " | Opp reshuffled";
            return "";
        }
    }
}
