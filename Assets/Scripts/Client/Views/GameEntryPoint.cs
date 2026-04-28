using System;
using System.Linq;
using UnityEngine;
using WarGame.Shared;

namespace WarGame.Client.Views
{
    public class GameEntryPoint : MonoBehaviour
    {
        [SerializeField] private GameBoard board;

        private GameController _gameController;

        private void Awake()
        {
            InitializeDesk();
            board.Deck.Shuffle();

            // TODO wait for it and enable input after
            var tween = board.DealCardsToPlayers();

            _gameController = new GameController(board);
            _gameController.Initialize();
        }

        private void OnDestroy()
        {
            _gameController.Dispose();
        }

        private void InitializeDesk() => board.Deck
                                              .Initialize(Enum
                                                          .GetValues(typeof(CardRank))
                                                          .Cast<CardRank>()
                                                          .SelectMany(rank => Enumerable.Repeat(rank, 4))
                                                          .ToList());
    }
}