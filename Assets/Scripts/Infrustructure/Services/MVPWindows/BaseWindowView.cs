using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Infrustructure.Services.UI;
using Infrustructure.Services.Windows.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace Infrustructure.Services.MVPWindows
{
    public abstract class BaseWindowView : MonoBehaviour, IWindowView
    {
        [SerializeField] private RectTransform content;
        [SerializeField] private List<Button> closeButtons;
        [SerializeField] private BaseWindowAnimationComponent[] animationComponents;
        [SerializeField] private WindowGuiLayer guiLayer = WindowGuiLayer.GUI;

        public event Action CloseClicked;

        public GameObject GameObject => gameObject;
        public Transform Transform => transform;
        public WindowGuiLayer GuiLayer => guiLayer;

        public virtual UniTask InitializeAsync()
        {
            if (closeButtons == null)
            {
                return UniTask.CompletedTask;
            }

            foreach (Button button in closeButtons)
            {
                if (button != null)
                {
                    button.onClick.AddListener(OnCloseClicked);
                }
            }

            return UniTask.CompletedTask;
        }

        public virtual async UniTask PlayOpenAsync()
        {
            SetVisible(true);

            var openTasks = new List<UniTask>();
            if (animationComponents == null)
            {
                return;
            }

            foreach (BaseWindowAnimationComponent animationComponent in animationComponents)
            {
                if (animationComponent != null)
                {
                    openTasks.Add(animationComponent.OpenAsync());
                }
            }

            await UniTask.WhenAll(openTasks);
        }

        public virtual async UniTask PlayCloseAsync()
        {
            var closeTasks = new List<UniTask>();
            if (animationComponents == null)
            {
                SetVisible(false);
                
                return;
            }

            foreach (BaseWindowAnimationComponent animationComponent in animationComponents)
            {
                if (animationComponent != null)
                {
                    closeTasks.Add(animationComponent.CloseAsync());
                }
            }

            await UniTask.WhenAll(closeTasks);
            
            SetVisible(false);
        }

        public void SetVisible(bool isVisible) =>
            gameObject.SetActive(isVisible);

        public void ResetTransform() =>
            content.localScale = Vector3.zero;

        private void OnCloseClicked() =>
            CloseClicked?.Invoke();
    }
}
