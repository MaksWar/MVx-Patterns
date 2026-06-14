using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Infrustructure.AssetManagement;
using UnityEngine;
using Zenject;

namespace Infrustructure.Services.MVPWindows
{
    public sealed class WindowFactory : IWindowFactory
    {
        private readonly DiContainer _diContainer;
        private readonly IAssetsProvider _assetProvider;
        private readonly List<IWindowPresenterFactory> _presenterFactories;

        public WindowFactory(DiContainer diContainer, IAssetsProvider assetProvider)
            : this(diContainer, assetProvider, new List<IWindowPresenterFactory>())
        {
        }

        [Inject]
        public WindowFactory(
            DiContainer diContainer,
            IAssetsProvider assetProvider,
            List<IWindowPresenterFactory> presenterFactories)
        {
            _diContainer = diContainer;
            _assetProvider = assetProvider;
            _presenterFactories = presenterFactories;
        }

        public async UniTask<WindowSession> CreateAsync(string windowName, IWindowsManager windowsManager)
        {
            GameObject prefab = await _assetProvider.Load<GameObject>(GetPrefabName(windowName), GetType());
            GameObject instance = _diContainer.InstantiatePrefab(prefab);
            instance.name = windowName;
            instance.SetActive(false);

            var view = instance.GetComponent<IWindowView>();
            if (view == null)
            {
                Debug.LogError($"{windowName} prefab root must have a component implementing IWindowView.");
                Object.Destroy(instance);
                
                return null;
            }

            IWindowPresenter presenter = CreatePresenter(windowName, view, windowsManager);
            if (presenter == null)
            {
                Object.Destroy(instance);
                
                return null;
            }

            await view.InitializeAsync();

            view.ResetTransform();

            return new WindowSession(windowName, instance, view, presenter);
        }

        private IWindowPresenter CreatePresenter(string windowName, IWindowView view, IWindowsManager windowsManager)
        {
            IWindowPresenterFactory presenterFactory = _presenterFactories.FirstOrDefault(factory => factory.CanCreate(windowName));
            if (presenterFactory == null)
            {
                Debug.LogError($"{windowName} has no registered IWindowPresenterFactory.");
                
                return null;
            }

            return presenterFactory.Create(windowName, view, windowsManager);
        }

        private static string GetPrefabName(string windowName) =>
            $"MVPWindows/{windowName}.prefab";
    }
}
