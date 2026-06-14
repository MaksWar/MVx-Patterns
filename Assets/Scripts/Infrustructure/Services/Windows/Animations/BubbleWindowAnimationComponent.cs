using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Infrustructure.Services.Windows.Animations
{
    public class BubbleWindowAnimationComponent : BaseWindowAnimationComponent
    {
        [SerializeField] private float closedScale;
        [SerializeField] private float openedScale;

        public override void Reset() =>
            CanvasGroup.transform.localScale = Vector3.one;

        public override void ForcedClose() =>
            CanvasGroup.transform.localScale = Vector3.zero;

        protected override async UniTask AnimateOpenAsync()
        {
            CanvasGroup.transform.localScale = Vector3.one * openedScale;

            await CanvasGroup.transform.DOScale(1, OpenDuration).SetEase(OpenAnimationCurve)
                .SetDelay(waitTimeBeforeStart);
        }

        protected override async UniTask AnimateCloseAsync(bool disable)
        {
            await CanvasGroup.transform.DOScale(closedScale, CloseDuration).SetEase(CloseAnimationCurve);

            if (disable)
            {
                gameObject.SetActive(false);
            }
        }
    }
}