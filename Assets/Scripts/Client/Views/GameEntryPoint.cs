using UnityEngine;
using WarGame.Client.Animation;
using WarGame.Server;
using WarGame.Shared;

namespace WarGame.Client.Views
{
    public class GameEntryPoint : MonoBehaviour
    {
        [SerializeField] private GameBoard board;

        // Can use SerializeReference and select IDealStrategy, but we need to write custom inspector for that
        [SerializeField] private DebugPreset debugPreset;

        private GameController _gameController;

        private async void Awake()
        {
            var inputManager = gameObject.AddComponent<InputManager>();
            inputManager.DisableInput();

            IDealStrategy strategy = debugPreset switch
            {
                DebugPreset.PlayerWinsRound1     => new OneRoundWinStrategy(true),
                DebugPreset.OpponentWinsRound1   => new OneRoundWinStrategy(false),
                DebugPreset.TripleWar            => new TripleWarStrategy(),
                DebugPreset.WarInsufficientCards => new WarInsufficientCardsStrategy(),
                _                                => new RandomDealStrategy()
            };

            var server = new FakeWarServer(strategy);
            var client = new GameClient(server);

            if (debugPreset != DebugPreset.None)
                server.DeleteSave();

            var state = await client.StartGameAsync();

            if (state.Status != ResponseStatus.Success)
            {
                Debug.LogError($"[GameEntryPoint] Failed to start game: {state.ErrorMessage}");
                return;
            }

            board.Deck.Initialize(state.TotalCards);

            if (state.IsRestoredGame)
                board.RestoreFromState(state);
            else
                await board.DealCardsToPlayers(state.PlayerHandCount, state.OpponentHandCount)
                           .ToUniTask();

            _gameController = new GameController(board, client, inputManager);
            _gameController.Initialize();
            inputManager.EnableInput();
        }

        private void OnDestroy()
        {
            _gameController?.Dispose();
        }
    }
}