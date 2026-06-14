using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Infrustructure.Services.Windows.Animations
{
    public class DownTopAnimationComponent : BaseWindowAnimationComponent
    {
        [SerializeField] private Transform contentTransform;
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;
        [SerializeField] private Ease openEase = Ease.OutBack;
        [SerializeField] private Ease closeEase = Ease.InBack;
        [SerializeField] private float delayBeforeClose;
        
        public override void ForcedClose()
        {
        }

        public override void Reset()
        {
            contentTransform.position = startPoint.position;
        }

        public override void ForcedOpen()
        {
            if (contentTransform != null && endPoint != null)
            {
                contentTransform.position = endPoint.position;
            }
        }

        protected override UniTask AnimateOpenAsync()
        {
            Reset();
            
            return contentTransform
                .DOMove(endPoint.position, OpenDuration)
                .SetEase(openEase)
                .SetDelay(waitTimeBeforeStart)
                .ToUniTask();
        }

        protected override UniTask AnimateCloseAsync(bool disable)
        {
            return contentTransform
                .DOMove(startPoint.position, CloseDuration)
                .SetEase(closeEase)
                .SetDelay(delayBeforeClose)
                .ToUniTask();
        }
    }
}
