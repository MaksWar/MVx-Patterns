using System;
using System.Collections.Generic;
using Infrustructure.Services.MVPWindows.Examples.Inventory.Model;
using Infrustructure.Services.MVPWindows.Examples.Inventory.View;

namespace Infrustructure.Services.MVPWindows.Examples.Inventory.Presenter
{
    public sealed class InventoryListPresenter : IDisposable
    {
        private readonly InventoryListView _view;

        public InventoryListPresenter(InventoryListView view)
        {
            _view = view;
            _view.ItemClicked += OnItemClicked;
        }

        public event Action<InventoryItemData> OpenItemRequested;
        public event Action<InventoryItemData> LockedItemClicked;

        public void ShowItems(IReadOnlyList<InventoryItemData> items) =>
            _view.Bind(items);

        public void Dispose() =>
            _view.ItemClicked -= OnItemClicked;

        private void OnItemClicked(InventoryItemData item)
        {
            if (item.State == InventoryItemState.Locked)
            {
                LockedItemClicked?.Invoke(item);
                
                return;
            }

            OpenItemRequested?.Invoke(item);
        }
    }
}
