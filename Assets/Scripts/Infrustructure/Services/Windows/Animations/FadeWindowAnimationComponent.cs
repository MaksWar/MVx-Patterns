using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Infrustructure.Services.Windows.Animations
{
    public class FadeWindowAnimationComponent : BaseWindowAnimationComponent
    {
        public override void Reset() =>
            CanvasGroup.alpha = 1;

        public override void ForcedClose() =>
            CanvasGroup.alpha = 0;

        protected override async UniTask AnimateOpenAsync()
        {
            CanvasGroup.transform.localScale = Vector3.one;

            CanvasGroup.alpha = 0;
            
            await UniTask.Delay(TimeSpan.FromSeconds(waitTimeBeforeStart));
            
            CanvasGroup.alpha = .3f;

            await CanvasGroup.DOFade(1f, OpenDuration).SetEase(Ease.OutCubic).SetUpdate(true);
        }

        protected override async UniTask AnimateCloseAsync(bool disable)
        {
            var fadeDelay = CloseDuration * 0.5f;

            await CanvasGroup.DOFade(0f, CloseDuration - fadeDelay).SetEase(Ease.InCubic).SetUpdate(true)
                .SetDelay(fadeDelay);

            if (disable)
            {
                gameObject.SetActive(false);
            }
        }
    }
}