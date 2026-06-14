using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Infrustructure.Services.UI;
using Infrustructure.Services.Windows.Animations;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Infrustructure.Services.Windows
{
    public abstract class BaseWindowController : MonoBehaviour
    {
        [SerializeField] private RectTransform content;

        [SerializeField] private List<Button> closeButtons;

        [SerializeField] private BaseWindowAnimationComponent[] animationComponents;
        [SerializeField] private WindowGuiLayer guiLayer = WindowGuiLayer.GUI;

        private IWindowsManager WindowsManager;

        protected BaseWindowParams Params;

        public WindowGuiLayer GuiLayer => guiLayer;


        [Inject]
        private void Construct(IWindowsManager windowsManager)
        {
            WindowsManager = windowsManager;
        }

        public virtual async UniTask InitializeAsync()
        {
            foreach (var button in closeButtons)
            {
                button.onClick.AddListener(() => FireClose().Forget());
            }
        }

        public virtual async UniTask OpenWindowAsync()
        {       
            gameObject.SetActive(true);
            
            var openTasks = new List<UniTask>();
            foreach (BaseWindowAnimationComponent animationComponent in animationComponents)
            {
                openTasks.Add(animationComponent.OpenAsync());
            }

            await UniTask.WhenAll(openTasks);
        }

        protected void InstantOpenWindow()
        {
            gameObject.SetActive(true);

            if (animationComponents != null && animationComponents.Length > 0)
            {
                foreach (BaseWindowAnimationComponent animationComponent in animationComponents)
                {
                    animationComponent.ForcedOpen();
                }
            }
            else
            {
                BeforeOpen();
            }
        }

        public virtual async UniTask CloseWindowAsync()
        {
            var closeTasks = new List<UniTask>();
            foreach (BaseWindowAnimationComponent animationComponent in animationComponents)
            {
                closeTasks.Add(animationComponent.CloseAsync());
            }

            await UniTask.WhenAll(closeTasks);

            gameObject.SetActive(false);
        }

        protected void InstantCloseWindow()
        {
            if (animationComponents != null && animationComponents.Length > 0)
            {
                foreach (BaseWindowAnimationComponent animationComponent in animationComponents)
                {
                    animationComponent.ForcedClose();
                }
            }

            gameObject.SetActive(false);
        }

        public virtual UniTask BeforeOpenAsync()
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask AfterOpenAsync()
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask BeforeCloseAsync()
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask AfterCloseAsync()
        {
            return UniTask.CompletedTask;
        }

        public virtual void BeforeClose()
        {
        }

        public virtual void AfterClose()
        {
        }

        public virtual void BeforeOpen()
        {
            if (content.GetComponent<BaseWindowAnimationComponent>() == null)
            {
                content.localScale = Vector3.one;
            }
        }

        public virtual void AfterOpen()
        {
        }

        public virtual void HandleParams(BaseWindowParams customParams)
        {
            Params = customParams;
        }

        public UniTask FireClose() =>
            WindowsManager.CloseWindowAsync(gameObject.name);

        public void ResetTransform() =>
            content.localScale = Vector3.zero;
    }

    public abstract class BaseWindowController<T> : BaseWindowController where T : BaseWindowParams
    {
        public T CustomParams
        {
            get
            {
                if (Params == null)
                {
                    Params = new BaseWindowParams();
                }
                
                if (!(Params is T))
                {
                    var customParams = JObject.FromObject(Params.SpecialOptions).ToObject<T>();
                    customParams.CopyBaseParams(Params);
                    Params = customParams;
                }

                return Params as T;
            }
        }

        public override void AfterClose()
        {
            base.AfterClose();

            CustomParams?.OnClose?.Invoke();
        }

        public override void AfterOpen()
        {
            base.AfterOpen();

            CustomParams?.OnOpened?.Invoke();
        }
    }

    [Serializable]
    public class BaseWindowParams
    {
        public Transform parent;
        public WindowGuiLayer GuiLayer = WindowGuiLayer.GUI;
        public bool HasCustomGuiLayer;

        public Action OnClose;
        public Action OnOpened;

        public bool IsTutorial;

        public Dictionary<string, object> SpecialOptions = new Dictionary<string, object>();

        public BaseWindowParams()
        {
        }

        public BaseWindowParams(Dictionary<string, object> specialOptions)
        {
            SpecialOptions = specialOptions;
        }

        public virtual void CopyBaseParams(BaseWindowParams baseWindowParams)
        {
            SpecialOptions = baseWindowParams.SpecialOptions;
            parent = baseWindowParams.parent;
            GuiLayer = baseWindowParams.GuiLayer;
            HasCustomGuiLayer = baseWindowParams.HasCustomGuiLayer;
        }
    }
}
