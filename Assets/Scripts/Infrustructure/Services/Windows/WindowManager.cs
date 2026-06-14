using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Infrustructure.AssetManagement;
using Infrustructure.Services.UI;
using UnityEngine;
using Zenject;

namespace Infrustructure.Services.Windows
{
    public class WindowsManager : IWindowsManager
    {
        private readonly DiContainer _diContainer;
        private readonly IAssetsProvider _assetProvider;
        private readonly IWindowRootProvider _windowRootProvider;
        private readonly IUIRootInitializer _uiRootInitializer;

        private readonly Dictionary<string, BaseWindowController> _windowInstances = new();

        private string _windowInProcess;
        private BaseWindowController _currentWindow;

        public WindowsManager(DiContainer diContainer, IAssetsProvider assetProvider)
            : this(diContainer, assetProvider, new WindowRootProvider(), new UIRootInitializer(diContainer, assetProvider))
        {
        }

        [Inject]
        public WindowsManager(
            DiContainer diContainer,
            IAssetsProvider assetProvider,
            IWindowRootProvider windowRootProvider,
            IUIRootInitializer uiRootInitializer)
        {
            _assetProvider = assetProvider;
            _diContainer = diContainer;
            _windowRootProvider = windowRootProvider;
            _uiRootInitializer = uiRootInitializer;
        }

        public UniTask InitializeAsync() =>
            _uiRootInitializer.InitializeAsync();

        public async UniTask OpenWindowAsync(string windowName, BaseWindowParams customParams = null)
        {
            Debug.Log($"Open window: {windowName}");

            _windowInProcess = windowName;

            if (_windowInstances.TryGetValue(windowName, out BaseWindowController windowInstance))
            {
                await Open(windowInstance);
                _windowInProcess = default;
                return;
            }

            BaseWindowController windowController = await InstantiateWindowAsync(windowName);
            await Open(windowController);

            _windowInProcess = default;

            Debug.Log($"Opened window: {windowName}");

            async UniTask Open(BaseWindowController window)
            {
                await TryCloseCurrentWindow();

                _currentWindow = window;
                
                window.HandleParams(customParams);
                MoveWindowToTargetParent(window, customParams);
                window.BeforeOpen();
                await window.BeforeOpenAsync();

                await window.OpenWindowAsync();

                window.AfterOpen();
                await window.AfterOpenAsync();
            }
        }

        public async UniTask CloseWindowAsync(string windowName)
        {        
            if (_windowInstances.TryGetValue(windowName, out BaseWindowController windowInstance))
            {
                windowInstance.BeforeClose();
                
                await windowInstance.BeforeCloseAsync();
                
                await windowInstance.CloseWindowAsync();
                
                windowInstance.AfterClose();
                
                await windowInstance.AfterCloseAsync();
                
                _currentWindow = null;
            }
        }

        public async UniTask CloseAllWindows()
        {
            foreach (BaseWindowController window in _windowInstances.Values)
            {
                await window.FireClose();
            }

            _currentWindow = null;
        }

        public T GetCurrentWindow<T>() where T : BaseWindowController
        {
            if (_currentWindow is T currentWindow)
            {
                return currentWindow;
            }

            return null;
        }

        public bool IsWindowInProcessOrOpened(string windowName) =>
            IsWindowInProcess(windowName) || IsWindowOpened(windowName);

        public bool IsWindowInProcess(string windowName) =>
            _windowInProcess == windowName;

        public bool IsWindowOpened() =>
            _currentWindow != null;

        public bool IsWindowOpened(string windowName) =>
            _currentWindow?.gameObject.name == windowName;

        public void Unload() =>
            _windowInstances.Clear();

        private async UniTask<BaseWindowController> InstantiateWindowAsync(string windowName)
        {
            GameObject prefab = await GetWindowPrefab(windowName);
            Transform defaultParent = GetWindowParent(GetDefaultGuiLayer(prefab));
            GameObject instance = _diContainer.InstantiatePrefab(prefab, defaultParent);

            instance.name = windowName;
            instance.SetActive(false);

            var window = instance.GetComponent<BaseWindowController>();
            await window.InitializeAsync();
            window.ResetTransform();

            _windowInstances.Add(windowName, window);

            return window;
        }

        private void MoveWindowToTargetParent(BaseWindowController window, BaseWindowParams customParams)
        {
            if (window == null)
            {
                return;
            }

            Transform targetParent = customParams?.parent;
            if (targetParent == null)
            {
                WindowGuiLayer targetLayer = customParams is { HasCustomGuiLayer: true }
                    ? customParams.GuiLayer
                    : window.GuiLayer;
                targetParent = GetWindowParent(targetLayer);
            }

            if (targetParent == null || window.transform.parent == targetParent)
            {
                return;
            }

            window.transform.SetParent(targetParent, false);
            window.ResetTransform();
        }

        private Transform GetWindowParent(WindowGuiLayer guiLayer) =>
            _windowRootProvider.GetRoot(guiLayer);

        private static WindowGuiLayer GetDefaultGuiLayer(GameObject prefab)
        {
            BaseWindowController windowController = prefab.GetComponent<BaseWindowController>();
            return windowController != null
                ? windowController.GuiLayer
                : WindowGuiLayer.GUI;
        }

        private async UniTask TryCloseCurrentWindow()
        {
            if (_currentWindow != null)
            {
                await CloseWindowAsync(_currentWindow.gameObject.name);
            }
        }

        private UniTask<GameObject> GetWindowPrefab(string windowName)
        {
            var prefabName = GetPrefabName(windowName);

            return _assetProvider.Load<GameObject>(prefabName, GetType());
        }

        private string GetPrefabName(string windowName) =>
            $"Windows/{windowName}.prefab";
    }
}
