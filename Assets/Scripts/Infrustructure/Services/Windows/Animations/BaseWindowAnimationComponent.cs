using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Infrustructure.Services.Windows.Animations
{
    public abstract class BaseWindowAnimationComponent : MonoBehaviour, IWindowAnimationComponent
    {
        [SerializeField] private float customCloseDuration;
        [SerializeField] private float customOpenDuration;
       
        [Header("Вказать тип анимации окна")]
        [SerializeField] private AnimationType animate;
        
        [Header("Налаштувати криві анімації вручну")]
        [SerializeField] protected bool isManualAnimationCurve;
        [SerializeField] private AnimationCurve openCurve;
        [SerializeField] private AnimationCurve closeCurve;
        [Header("Налаштувати паузу перед початком анімації")]
        [SerializeField] protected float waitTimeBeforeStart;
        [Header("Наладить скорость анимации окна")]
        [SerializeField] protected bool isUseCustomSpeedAnimation = false;
        [SerializeField] protected bool enableOnOpen = true;

        private CanvasGroup _canvasGroup;

        protected virtual AnimationCurve DefaultOpenAnimationCurve => new AnimationCurve(
            new Keyframe(0f, 0, 0f, 0f),
            new Keyframe(0.332f, 0.94315517f, 0.6970305f, 0.6970305f),
            new Keyframe(0.6295148f, 0.9889103f, -0.034956377f, -0.034956377f),
            new Keyframe(1.0041789f, 1f, 0.18423282f, 0.18423282f)
        );

        protected virtual AnimationCurve DefaultCloseAnimationCurve => new AnimationCurve(
            new Keyframe(0f, 0f, -1.1544099f, -1.1544099f),
            new Keyframe(0.5f, 1f, 0.7319877f, 0.7319877f)
        );

        protected AnimationCurve OpenAnimationCurve => isManualAnimationCurve ? openCurve : DefaultOpenAnimationCurve;
        protected AnimationCurve CloseAnimationCurve => isManualAnimationCurve ? closeCurve : DefaultCloseAnimationCurve;

        protected float OpenDuration=> isUseCustomSpeedAnimation ? customOpenDuration : DefaultOpenDuration;
        protected float CloseDuration => isUseCustomSpeedAnimation ? customCloseDuration : DefaultCloseDuration;

        protected virtual float DefaultOpenDuration => 0.5f;
        protected virtual float DefaultCloseDuration => 0.357f;

        protected CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup != null)
                {
                    return _canvasGroup;
                }

                _canvasGroup = gameObject.GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }

                return _canvasGroup;
            }
        }
        
        public async UniTask OpenAsync()
        {
            if (enableOnOpen)
            {
                gameObject.SetActive(true);
            }

            if (animate.HasFlag(AnimationType.Open))
            {
                await AnimateOpenAsync();
            }
        }

        public async UniTask CloseAsync(bool disable = false, bool ignoreHasFlagCheck = false)
        {
            if (ignoreHasFlagCheck || animate.HasFlag(AnimationType.Close))
            {
                await AnimateCloseAsync(disable);
            }
        }

        public virtual void ForcedOpen() => Reset();
        public abstract void ForcedClose();
        public abstract void Reset();

        protected abstract UniTask AnimateOpenAsync();
        protected abstract UniTask AnimateCloseAsync(bool disable);
    }

    [Flags]
    public enum AnimationType
    {
        Open = 1,
        Close = 2
    }
}
