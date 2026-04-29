using System;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame.Client.Views
{
    public class CardDeck : MonoBehaviour
    {
        [SerializeField] private RectTransform cardParent;
        [SerializeField] private CardView cardPrefab;

        public IReadOnlyList<CardView> Cards => _cards;
        public bool IsEmpty => _cards.Count == 0;

        private readonly List<CardView> _cards = new();

        // Creates count face-down cards with unknown rank (rank is set by the server on reveal)
        public void Initialize(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var card = Instantiate(cardPrefab, cardParent);
                card.Initialize(null, false);
                _cards.Add(card);
            }
        }

        public CardView GetCard()
        {
            if (IsEmpty)
                throw new Exception("Card Deck is empty");

            var topCard = _cards[_cards.Count - 1];
            topCard.transform.SetParent(null);
            _cards.RemoveAt(_cards.Count - 1);
            return topCard;
        }
    }
}
