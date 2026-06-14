using Cysharp.Threading.Tasks;

namespace Infrustructure.Services.MVPWindows
{
    public interface IWindowFactory
    {
        UniTask<WindowSession> CreateAsync(string windowName, IWindowsManager windowsManager);
    }
}
