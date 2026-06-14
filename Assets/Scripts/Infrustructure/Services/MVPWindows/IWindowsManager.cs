using Cysharp.Threading.Tasks;

namespace Infrustructure.Services.MVPWindows
{
    public interface IWindowsManager
    {
        UniTask InitializeAsync();
        UniTask OpenWindowAsync(string windowName, WindowArgs args = null, WindowOptions options = null);
        UniTask CloseWindowAsync(string windowName);
        UniTask CloseAllWindows();

        bool IsWindowOpened();
        bool IsWindowOpened(string windowName);
        bool IsWindowInProcess(string windowName);
        bool IsWindowInProcessOrOpened(string windowName);

        TView GetCurrentWindowView<TView>() where TView : class, IWindowView;
        void Unload();
    }
}
