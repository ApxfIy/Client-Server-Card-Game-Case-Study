using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace WarGame.Client.Views
{
    public class CardsContainerView : MonoBehaviour
    {
        [SerializeField] private RectTransform container;

        public event Action<CardView, CardsContainerView> OnClick;

        public int Count => _cards.Count;
        public IReadOnlyList<CardView> Cards => _cards;

        private readonly List<CardView> _cards = new();

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

        public bool RemoveCard(CardView cardView)
        {
            cardView.OnClick -= OnCardClick;
            return _cards.Remove(cardView);
        }

        private void OnCardClick(CardView cardView)
        {
            OnClick?.Invoke(cardView, this);
        }
    }
}
