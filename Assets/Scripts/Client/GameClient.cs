using System;
using Cysharp.Threading.Tasks;
using WarGame.Server;
using WarGame.Shared;

namespace WarGame.Client
{
    public class GameClient
    {
        private readonly FakeWarServer _server = new();
        private const int MaxRetries   = 3;
        private const int RetryDelayMs = 600;

        public event Action<string> OnNetworkStatusChanged;

        public async UniTask<StartGameResponse> StartGameAsync()
        {
            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                GameLogger.Client(attempt == 0
                    ? "→ StartGame"
                    : $"→ StartGame retry ({attempt}/{MaxRetries - 1})");

                if (attempt > 0)
                    OnNetworkStatusChanged?.Invoke($"Retrying... ({attempt}/{MaxRetries - 1})");

                try
                {
                    var response = await _server.StartGameAsync();

                    if (attempt > 0) OnNetworkStatusChanged?.Invoke(string.Empty);

                    string kind = response.IsRestoredGame ? "restored" : "new game";
                    string war = response.IsWarActive
                        ? $" | war active (pot: {response.WarFaceDownCount}, slots: {response.PlayerSlotRanks?.Length ?? 0} each)"
                        : "";
                    GameLogger.Client(
                        $"← StartGame [{kind}]: " +
                        $"Player hand: {response.PlayerHandCount} captured: {response.PlayerCapturedCount} | " +
                        $"Opp hand: {response.OpponentHandCount} captured: {response.OpponentCapturedCount}{war}");

                    return response;
                }
                catch (TimeoutException ex)
                {
                    GameLogger.Client($"← StartGame: TIMEOUT — {ex.Message}");
                    return new StartGameResponse { Status = ResponseStatus.Timeout, ErrorMessage = ex.Message };
                }
                catch (Exception ex)
                {
                    bool isLast = attempt == MaxRetries - 1;
                    GameLogger.Client(isLast
                        ? $"← StartGame: NETWORK ERROR — giving up after {MaxRetries} attempts ({ex.Message})"
                        : $"← StartGame: network error (attempt {attempt + 1}/{MaxRetries}), retrying in {RetryDelayMs}ms...");

                    if (isLast)
                        return new StartGameResponse { Status = ResponseStatus.NetworkError, ErrorMessage = "Connection failed" };

                    await UniTask.Delay(RetryDelayMs);
                }
            }

            GameLogger.Client($"← StartGame: FAILED — max retries exceeded");
            return new StartGameResponse { Status = ResponseStatus.NetworkError, ErrorMessage = "Max retries exceeded" };
        }

        public async UniTask<PlayRoundResponse> PlayRoundAsync()
        {
            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                GameLogger.Client(attempt == 0
                    ? "→ PlayRound"
                    : $"→ PlayRound retry ({attempt}/{MaxRetries - 1})");

                if (attempt > 0)
                    OnNetworkStatusChanged?.Invoke($"Retrying... ({attempt}/{MaxRetries - 1})");

                try
                {
                    var response = await _server.PlayRoundAsync();

                    if (attempt > 0) OnNetworkStatusChanged?.Invoke(string.Empty);

                    LogPlayRoundResponse(response);
                    return response;
                }
                catch (TimeoutException ex)
                {
                    GameLogger.Client($"← PlayRound: TIMEOUT — {ex.Message}");
                    return new PlayRoundResponse { Status = ResponseStatus.Timeout, ErrorMessage = ex.Message };
                }
                catch (Exception ex)
                {
                    bool isLast = attempt == MaxRetries - 1;
                    GameLogger.Client(isLast
                        ? $"← PlayRound: NETWORK ERROR — giving up after {MaxRetries} attempts ({ex.Message})"
                        : $"← PlayRound: network error (attempt {attempt + 1}/{MaxRetries}), retrying in {RetryDelayMs}ms...");

                    if (isLast)
                        return new PlayRoundResponse { Status = ResponseStatus.NetworkError, ErrorMessage = "Connection failed" };

                    await UniTask.Delay(RetryDelayMs);
                }
            }

            GameLogger.Client($"← PlayRound: FAILED — max retries exceeded");
            return new PlayRoundResponse { Status = ResponseStatus.NetworkError, ErrorMessage = "Max retries exceeded" };
        }

        private static void LogPlayRoundResponse(PlayRoundResponse r)
        {
            if (r.EarlyGameOver)
            {
                GameLogger.Client($"← PlayRound: GAME OVER (early, no cards played) — {r.FinalResult}");
                return;
            }

            string war = r.PlayerWarCardsPlayed > 0
                ? $" | war: +{r.PlayerWarCardsPlayed} face-down each"
                : "";
            string reshuffle = BuildReshuffleNote(r.PlayerHandReshuffled, r.OpponentHandReshuffled);
            string counts =
                $"Player hand: {r.PlayerHandCount} captured: {r.PlayerCapturedCount} | " +
                $"Opp hand: {r.OpponentHandCount} captured: {r.OpponentCapturedCount}";
            string gameOver = r.IsGameOver ? $" | GAME OVER — {r.FinalResult}" : "";

            GameLogger.Client(
                $"← PlayRound [{r.RoundResult}]: " +
                $"Player {r.PlayerCard.RankToString()} vs Opp {r.OpponentCard.RankToString()}" +
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
