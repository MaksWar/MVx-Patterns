using Cysharp.Threading.Tasks;

namespace Infrustructure.Services.Windows
{
    public interface IWindowsManager
    {
        UniTask OpenWindowAsync(string windowName, BaseWindowParams customParams = null);
        UniTask CloseWindowAsync(string windowName);
        bool IsWindowOpened();
        bool IsWindowOpened(string windowName);
        bool IsWindowInProcess(string windowName);
        void Unload();
        UniTask InitializeAsync();
        UniTask CloseAllWindows();
        bool IsWindowInProcessOrOpened(string windowName);
        T GetCurrentWindow<T>() where T : BaseWindowController;
    }
}
