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
                if (attempt > 0)
                    OnNetworkStatusChanged?.Invoke($"Retrying... ({attempt}/{MaxRetries - 1})");

                try
                {
                    var response = await _server.StartGameAsync();
                    if (attempt > 0) OnNetworkStatusChanged?.Invoke(string.Empty);
                    return response;
                }
                catch (TimeoutException ex)
                {
                    return new StartGameResponse { Status = ResponseStatus.Timeout, ErrorMessage = ex.Message };
                }
                catch (Exception)
                {
                    if (attempt == MaxRetries - 1)
                        return new StartGameResponse { Status = ResponseStatus.NetworkError, ErrorMessage = "Connection failed" };
                    await UniTask.Delay(RetryDelayMs);
                }
            }

            return new StartGameResponse { Status = ResponseStatus.NetworkError, ErrorMessage = "Max retries exceeded" };
        }

        public async UniTask<PlayRoundResponse> PlayRoundAsync()
        {
            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                if (attempt > 0)
                    OnNetworkStatusChanged?.Invoke($"Retrying... ({attempt}/{MaxRetries - 1})");

                try
                {
                    var response = await _server.PlayRoundAsync();
                    if (attempt > 0) OnNetworkStatusChanged?.Invoke(string.Empty);
                    return response;
                }
                catch (TimeoutException ex)
                {
                    return new PlayRoundResponse { Status = ResponseStatus.Timeout, ErrorMessage = ex.Message };
                }
                catch (Exception)
                {
                    if (attempt == MaxRetries - 1)
                        return new PlayRoundResponse { Status = ResponseStatus.NetworkError, ErrorMessage = "Connection failed" };
                    await UniTask.Delay(RetryDelayMs);
                }
            }

            return new PlayRoundResponse { Status = ResponseStatus.NetworkError, ErrorMessage = "Max retries exceeded" };
        }
    }
}
