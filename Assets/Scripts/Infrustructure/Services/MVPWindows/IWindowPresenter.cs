using System;
using Cysharp.Threading.Tasks;

namespace Infrustructure.Services.MVPWindows
{
    public interface IWindowPresenter : IDisposable
    {
        UniTask OpenAsync(WindowArgs args, WindowOptions options);
        UniTask CloseAsync();
    }
}
