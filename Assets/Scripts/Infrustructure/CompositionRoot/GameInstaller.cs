using Infrustructure.AssetManagement;
using Zenject;

namespace Infrustructure.CompositionRoot
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IAssetsProvider>().To<AssetsProvider>().AsSingle();
        }
    }
}
