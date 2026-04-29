using UnityEngine;

namespace WarGame.Client.Animation
{
    [CreateAssetMenu(fileName = "AnimationConfig", menuName = "WarGame/Animation Config")]
    public class AnimationConfig : ScriptableObject
    {
        [SerializeField] private float cardFlipDuration = 0.2f;
        [SerializeField] private float dealCardDuration = 0.2f;
        [SerializeField] private float battleSlotDuration = 0.5f;
        [SerializeField] private float collectCardsDuration = 0.3f;
        [SerializeField] private float reshuffleDuration = 0.2f;
        [SerializeField] private int revealResultDelayMs = 500;

        public float CardFlipDuration => cardFlipDuration;
        public float DealCardDuration => dealCardDuration;
        public float BattleSlotDuration => battleSlotDuration;
        public float CollectCardsDuration => collectCardsDuration;
        public float ReshuffleDuration => reshuffleDuration;
        public int RevealResultDelayMs => revealResultDelayMs;
    }
}
