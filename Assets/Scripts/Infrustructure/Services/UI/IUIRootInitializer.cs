using Cysharp.Threading.Tasks;

namespace Infrustructure.Services.UI
{
    public interface IUIRootInitializer
    {
        UniTask InitializeAsync();
    }
}
