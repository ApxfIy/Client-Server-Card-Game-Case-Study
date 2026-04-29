using UnityEngine;
using WarGame.Client.Animation;
using WarGame.Shared;

namespace WarGame.Client.Views
{
    public class GameEntryPoint : MonoBehaviour
    {
        [SerializeField] private GameBoard board;

        private GameController _gameController;

        private async void Awake()
        {
            var client = new GameClient();
            var state = await client.StartGameAsync();

            if (state.Status != ResponseStatus.Success)
            {
                Debug.LogError($"[GameEntryPoint] Failed to start game: {state.ErrorMessage}");
                return;
            }

            // Total is always 52: hands + captured piles + war pot + revealed slot cards
            int totalCards = state.PlayerHandCount
                           + state.OpponentHandCount
                           + state.PlayerCapturedCount
                           + state.OpponentCapturedCount
                           + state.WarFaceDownCount
                           + (state.PlayerSlotRanks?.Length ?? 0)
                           + (state.OpponentSlotRanks?.Length ?? 0);

            board.Deck.Initialize(totalCards);

            if (state.IsRestoredGame)
            {
                board.RestoreFromState(state);
            }
            else
            {
                await board.DealCardsToPlayers(state.PlayerHandCount, state.OpponentHandCount)
                           .ToUniTask();
            }

            _gameController = new GameController(board, client);
            _gameController.Initialize();
        }

        private void OnDestroy()
        {
            _gameController?.Dispose();
        }
    }
}
