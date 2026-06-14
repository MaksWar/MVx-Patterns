using Cysharp.Threading.Tasks;

namespace Infrustructure.Services.Windows
{
    interface IWindowAnimationComponent
    {
        UniTask OpenAsync();
        UniTask CloseAsync(bool disable = false, bool ignoreHasFlagCheck = false);
        void Reset();
    }
}
