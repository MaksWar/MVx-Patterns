using UnityEngine;
using Zenject;

namespace Infrustructure.Services.MVPWindows
{
    public abstract class BaseWindowPresenterFactory<TView, TModel, TPresenter> : IWindowPresenterFactory
        where TView : class, IWindowView
        where TModel : IWindowModel
        where TPresenter : IWindowPresenter
    {
        private readonly DiContainer _diContainer;

        protected BaseWindowPresenterFactory(DiContainer diContainer)
        {
            _diContainer = diContainer;
        }

        protected abstract string WindowName { get; }

        public bool CanCreate(string windowName) =>
            WindowName == windowName;

        public IWindowPresenter Create(string windowName, IWindowView view, IWindowsManager windowsManager)
        {
            if (!CanCreate(windowName))
            {
                Debug.LogError($"{GetType().Name} cannot create presenter for {windowName}.");
                
                return null;
            }

            if (view is not TView typedView)
            {
                Debug.LogError($"{windowName} view must be {typeof(TView).Name}.");
                
                return null;
            }

            TModel model = _diContainer.Instantiate<TModel>();
            
            return _diContainer.Instantiate<TPresenter>(new object[] { typedView, model, windowsManager });
        }
    }
}
