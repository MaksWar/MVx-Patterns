using Cysharp.Threading.Tasks;

namespace Infrustructure.Services.MVPWindows
{
    public abstract class BaseWindowPresenter<TView, TModel, TArgs> : IWindowPresenter
        where TView : IWindowView
        where TModel : BaseWindowModel<TArgs>
        where TArgs : WindowArgs, new()
    {
        private readonly IWindowsManager _windowsManager;

        protected readonly TView View;
        protected readonly TModel Model;

        protected BaseWindowPresenter(TView view, TModel model, IWindowsManager windowsManager)
        {
            View = view;
            Model = model;

            _windowsManager = windowsManager;

            View.CloseClicked += OnCloseClicked;
        }

        public async UniTask OpenAsync(WindowArgs args, WindowOptions options)
        {
            var typedArgs = args as TArgs ?? new TArgs();
            Model.Initialize(typedArgs, options);

            BeforeOpen();

            await BeforeOpenAsync();
            await View.PlayOpenAsync();

            AfterOpen();

            await AfterOpenAsync();

            options?.OnOpened?.Invoke();
        }

        public async UniTask CloseAsync()
        {
            BeforeClose();
            
            await BeforeCloseAsync();
            await View.PlayCloseAsync();

            AfterClose();
            
            await AfterCloseAsync();
            
            Model.Options?.OnClosed?.Invoke();
        }

        public virtual void Dispose() =>
            View.CloseClicked -= OnCloseClicked;

        protected virtual UniTask BeforeOpenAsync() =>
            UniTask.CompletedTask;

        protected virtual UniTask AfterOpenAsync() =>
            UniTask.CompletedTask;

        protected virtual UniTask BeforeCloseAsync() =>
            UniTask.CompletedTask;

        protected virtual UniTask AfterCloseAsync() =>
            UniTask.CompletedTask;

        protected virtual void BeforeOpen()
        {
        }

        protected virtual void AfterOpen()
        {
        }

        protected virtual void BeforeClose()
        {
        }

        protected virtual void AfterClose()
        {
        }

        private void OnCloseClicked() =>
            _windowsManager.CloseWindowAsync(View.GameObject.name).Forget();
    }
}
