using System;
using Cysharp.Threading.Tasks;
using Infrustructure.Services.MVPWindows.Examples.Inventory.Model;
using Infrustructure.Services.MVPWindows.Examples.Inventory.View;

namespace Infrustructure.Services.MVPWindows.Examples.Inventory.Presenter
{
    public sealed class InventoryDetailsPresenter : IDisposable
    {
        private readonly InventoryDetailsView _view;

        public InventoryDetailsPresenter(InventoryDetailsView view)
        {
            _view = view;
            _view.CloseClicked += OnCloseClicked;
        }

        public event Action BackRequested;

        public UniTask ShowAsync(InventoryItemData item) =>
            _view.PlayShowAsync(item);

        public UniTask HideAsync() =>
            _view.PlayHideAsync();

        public void Dispose() =>
            _view.CloseClicked -= OnCloseClicked;

        private void OnCloseClicked() =>
            BackRequested?.Invoke();
    }
}
