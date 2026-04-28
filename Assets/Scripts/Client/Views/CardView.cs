using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WarGame.Shared;

namespace WarGame.Client.Views
{
    public class CardView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private Image background;
        [SerializeField] private TextMeshProUGUI rankText;
        [SerializeField] private Color faceUpColor = Color.white;
        [SerializeField] private Color faceDownColor = new(0.18f, 0.36f, 0.72f);
        [SerializeField] private float animationDuration = 0.2f;

        public event Action<CardView> OnClick;
        public CardRank Rank { get; private set; }
        public bool IsFaceUp { get; private set; }

        public void Initialize(CardRank rank, bool faceUp)
        {
            Rank = rank;
            IsFaceUp = faceUp;
            ApplyVisuals(faceUp, rank);
        }

        [ContextMenu("FaceUp")]
        public Tween FaceUp()
        {
            if (IsFaceUp)
                return DOTween.Sequence();

            return DOTween.Sequence()
                   .Append(root.DOScaleX(0f, animationDuration).SetEase(Ease.InQuad))
                   .AppendCallback(() => ApplyVisuals(true, Rank))
                   .Append(root.DOScaleX(1f, animationDuration).SetEase(Ease.OutQuad))
                   .OnStart(() => IsFaceUp = true);
        }

        [ContextMenu("FaceDown")]
        public Tween FaceDown()
        {
            if (IsFaceUp == false)
                return DOTween.Sequence();

            return DOTween.Sequence()
                   .Append(root.DOScaleX(0f, animationDuration).SetEase(Ease.InQuad))
                   .AppendCallback(() => ApplyVisuals(false, Rank))
                   .Append(root.DOScaleX(1f, animationDuration).SetEase(Ease.OutQuad))
                   .OnStart(() => IsFaceUp = false);
        }

        private void ApplyVisuals(bool faceUp, CardRank rank)
        {
            rankText.enabled = faceUp;
            rankText.text = rank.RankToString();
            background.color = faceUp ? faceUpColor  : faceDownColor;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke(this);
            Debug.Log($"Card {Rank} clicked", this);
        }
    }
}
