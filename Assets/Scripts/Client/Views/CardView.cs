using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WarGame.Client.Animation;
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
        [SerializeField] private AnimationConfig animationConfig;

        public event Action<CardView> OnClick;

        // Null when the card is face-down and its rank is unknown to the client
        public CardRank? Rank { get; private set; }
        public bool IsFaceUp { get; private set; }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke(this);
        }

        public void Initialize(CardRank? rank, bool faceUp)
        {
            Rank = rank;
            IsFaceUp = faceUp;
            ApplyVisuals(faceUp, rank);
        }

        // Reveals the card with a rank received from the server
        public Tween Reveal(CardRank rank)
        {
            Rank = rank;
            return FaceUp();
        }

        [ContextMenu("FaceUp")]
        public Tween FaceUp()
        {
            if (IsFaceUp)
                return DOTween.Sequence();

            return DOTween.Sequence()
                          .Append(root.DOScaleX(0f, animationConfig.CardFlipDuration))
                          .AppendCallback(() => ApplyVisuals(true, Rank))
                          .Append(root.DOScaleX(1f, animationConfig.CardFlipDuration))
                          .OnStart(() => IsFaceUp = true);
        }

        [ContextMenu("FaceDown")]
        public Tween FaceDown()
        {
            if (!IsFaceUp)
                return DOTween.Sequence();

            return DOTween.Sequence()
                          .Append(root.DOScaleX(0f, animationConfig.CardFlipDuration))
                          .AppendCallback(() => ApplyVisuals(false, Rank))
                          .Append(root.DOScaleX(1f, animationConfig.CardFlipDuration))
                          .OnStart(() => IsFaceUp = false);
        }

        private void ApplyVisuals(bool faceUp, CardRank? rank)
        {
            rankText.enabled = faceUp && rank.HasValue;
            rankText.text = rank.HasValue ? rank.Value.RankToString() : string.Empty;
            background.color = faceUp ? faceUpColor : faceDownColor;
        }
    }
}