using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace WarGame.Client.Views
{
    public class CardsContainerView : MonoBehaviour
    {
        [SerializeField] private RectTransform container;

        public int Count => _cards.Count;
        public IReadOnlyList<CardView> Cards => _cards;

        private readonly List<CardView> _cards = new();

        public event Action<CardView, CardsContainerView> OnClick;

        public Sequence AddCard(CardView cardView, float duration)
        {
            return DOTween.Sequence()
                          .Append(cardView.transform.DOLocalMove(Vector3.zero, duration).SetEase(Ease.OutCubic))
                          .OnStart(() =>
                          {
                              cardView.transform.SetParent(container);
                              _cards.Add(cardView);
                              cardView.OnClick += OnCardClick;
                          });
        }

        public void AddCardImmediate(CardView cardView)
        {
            cardView.transform.SetParent(container);
            cardView.transform.localPosition = Vector3.zero;
            _cards.Add(cardView);
            cardView.OnClick += OnCardClick;
        }

        public bool RemoveCard(CardView cardView)
        {
            cardView.OnClick -= OnCardClick;
            return _cards.Remove(cardView);
        }

        public CardView TakeTopCard()
        {
            if (_cards.Count == 0) return null;

            var card = _cards[^1];
            RemoveCard(card);
            return card;
        }

        public List<CardView> TakeAllCards()
        {
            var all = _cards.ToList();
            _cards.Clear();

            foreach (var card in all)
                card.OnClick -= OnCardClick;

            return all;
        }

        private void OnCardClick(CardView cardView)
        {
            OnClick?.Invoke(cardView, this);
        }
    }
}