using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Infrustructure.AssetManagement;
using Infrustructure.Services.UI;
using UnityEngine;
using Zenject;

namespace Infrustructure.Services.MVPWindows
{
    public sealed class WindowsManager : IWindowsManager
    {
        private readonly IWindowFactory _windowFactory;
        private readonly IWindowRootProvider _windowRootProvider;
        private readonly IUIRootInitializer _uiRootInitializer;
        private readonly Dictionary<string, WindowSession> _windowInstances = new();
        private readonly List<WindowSession> _openedWindows = new();

        private string _windowInProcess;
        private WindowSession _currentWindow;

        public WindowsManager(DiContainer diContainer, IAssetsProvider assetProvider)
            : this(new WindowFactory(diContainer, assetProvider), new WindowRootProvider(), new UIRootInitializer(diContainer, assetProvider))
        {
        }

        [Inject]
        public WindowsManager(
            IWindowFactory windowFactory,
            IWindowRootProvider windowRootProvider,
            IUIRootInitializer uiRootInitializer)
        {
            _windowFactory = windowFactory;
            _windowRootProvider = windowRootProvider;
            _uiRootInitializer = uiRootInitializer;
        }

        public UniTask InitializeAsync() =>
            _uiRootInitializer.InitializeAsync();

        public async UniTask OpenWindowAsync(string windowName, WindowArgs args = null, WindowOptions options = null)
        {
            _windowInProcess = windowName;

            try
            {
                WindowSession session = await GetOrCreateWindowAsync(windowName);

                await TryCloseCurrentWindow();

                _currentWindow = session;
                
                AddOpenedWindow(session);
                MoveWindowToTargetParent(session.View, options);

                await session.Presenter.OpenAsync(args, options);
            }
            finally
            {
                _windowInProcess = default;
            }
        }

        public async UniTask CloseWindowAsync(string windowName)
        {
            if (!_windowInstances.TryGetValue(windowName, out WindowSession session))
            {
                return;
            }

            await session.Presenter.CloseAsync();
            
            _openedWindows.Remove(session);
            if (_currentWindow == session)
            {
                _currentWindow = GetLastOpenedWindow();
            }
        }

        public void Unload()
        {
            foreach (WindowSession session in _windowInstances.Values)
            {
                session.Dispose();
                Object.Destroy(session.GameObject);
            }

            _windowInstances.Clear();
            _openedWindows.Clear();
            _currentWindow = null;
        }

        public async UniTask CloseAllWindows()
        {
            var openedWindows = new List<WindowSession>(_openedWindows);
            for (int i = openedWindows.Count - 1; i >= 0; i--)
            {
                await CloseWindowAsync(openedWindows[i].Name);
            }
        }

        public bool IsWindowOpened() =>
            _currentWindow != null;

        public bool IsWindowOpened(string windowName) =>
            _currentWindow?.Name == windowName;

        public bool IsWindowInProcess(string windowName) =>
            _windowInProcess == windowName;

        public bool IsWindowInProcessOrOpened(string windowName) =>
            IsWindowInProcess(windowName) || IsWindowOpened(windowName);

        public TView GetCurrentWindowView<TView>() where TView : class, IWindowView =>
            _currentWindow?.View as TView;

        private async UniTask<WindowSession> GetOrCreateWindowAsync(string windowName)
        {
            if (_windowInstances.TryGetValue(windowName, out WindowSession session))
            {
                return session;
            }

            session = await _windowFactory.CreateAsync(windowName, this);
            
            _windowInstances.Add(windowName, session);
            
            return session;
        }

        private void MoveWindowToTargetParent(IWindowView view, WindowOptions options)
        {
            Transform targetParent = options?.Parent;
            if (targetParent == null)
            {
                targetParent = GetWindowParent(options?.GuiLayer ?? view.GuiLayer);
            }

            if (targetParent == null || view.Transform.parent == targetParent)
            {
                return;
            }

            view.Transform.SetParent(targetParent, false);
            view.ResetTransform();
        }

        private void AddOpenedWindow(WindowSession session)
        {
            _openedWindows.Remove(session);
            _openedWindows.Add(session);
        }

        private async UniTask TryCloseCurrentWindow()
        {
            if (_currentWindow != null)
            {
                await CloseWindowAsync(_currentWindow.Name);
            }
        }

        private Transform GetWindowParent(WindowGuiLayer guiLayer) =>
            _windowRootProvider.GetRoot(guiLayer);

        private WindowSession GetLastOpenedWindow() =>
            _openedWindows.Count > 0
                ? _openedWindows[^1]
                : null;
    }
}
