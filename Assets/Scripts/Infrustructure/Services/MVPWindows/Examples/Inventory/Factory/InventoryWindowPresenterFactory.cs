using Infrustructure.Services.MVPWindows.Examples.Inventory.Model;
using Infrustructure.Services.MVPWindows.Examples.Inventory.Presenter;
using Infrustructure.Services.MVPWindows.Examples.Inventory.View;
using UnityEngine;
using Zenject;

namespace Infrustructure.Services.MVPWindows.Examples.Inventory.Factory
{
    public sealed class InventoryWindowPresenterFactory
        : IWindowPresenterFactory
    {
        private readonly DiContainer _diContainer;

        public InventoryWindowPresenterFactory(DiContainer diContainer)
        {
            _diContainer = diContainer;
        }

        public bool CanCreate(string windowName) =>
            windowName == InventoryWindowView.PrefabName;

        public IWindowPresenter Create(string windowName, IWindowView view, IWindowsManager windowsManager)
        {
            if (!CanCreate(windowName))
            {
                Debug.LogError($"{nameof(InventoryWindowPresenterFactory)} cannot create presenter for {windowName}.");
                
                return null;
            }

            if (view is not InventoryWindowView inventoryView)
            {
                Debug.LogError($"{windowName} view must be {nameof(InventoryWindowView)}.");
                
                return null;
            }

            InventoryWindowModel model = _diContainer.Instantiate<InventoryWindowModel>();
            var listPresenter = new InventoryListPresenter(inventoryView.ListView);
            var detailsPresenter = new InventoryDetailsPresenter(inventoryView.DetailsView);

            return _diContainer.Instantiate<InventoryWindowPresenter>(
                new object[] { inventoryView, model, windowsManager, listPresenter, detailsPresenter });
        }
    }
}
