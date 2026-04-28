using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace WarGame.Client.Animation
{
    public static class TweenExtensions
    {
        public static UniTask ToUniTask(this Tween tween)
        {
            var tcs = new UniTaskCompletionSource();
            tween.OnComplete(() => tcs.TrySetResult())
                 .OnKill(() => tcs.TrySetResult());
            return tcs.Task;
        }

        public static UniTask ToUniTask(this Sequence sequence)
        {
            var tcs = new UniTaskCompletionSource();
            sequence.OnComplete(() => tcs.TrySetResult())
                    .OnKill(() => tcs.TrySetResult());
            return tcs.Task;
        }
    }
}
