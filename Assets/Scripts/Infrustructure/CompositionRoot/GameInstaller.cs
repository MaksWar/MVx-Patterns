using Infrustructure.AssetManagement;
using Infrustructure.Services.MVPWindows;
using Infrustructure.Services.MVPWindows.Examples.Inventory;
using Infrustructure.Services.MVPWindows.Examples.Inventory.Factory;
using Infrustructure.Services.UI;
using Zenject;

namespace Infrustructure.CompositionRoot
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IAssetsProvider>().To<AssetsProvider>().AsSingle();
            Container.Bind<IUIRootInitializer>().To<UIRootInitializer>().AsSingle();
            Container.Bind<IWindowRootProvider>().To<WindowRootProvider>().AsSingle();
            Container.Bind<IWindowFactory>().To<WindowFactory>().AsSingle();
            Container.Bind<IWindowsManager>().To<WindowsManager>().AsSingle();
            Container.Bind<IWindowPresenterFactory>().To<InventoryWindowPresenterFactory>().AsSingle();
        }
    }
}
