using Cysharp.Threading.Tasks;
using Infrustructure.Services.MVPWindows.Examples.Inventory.Model;
using Infrustructure.Services.MVPWindows.Examples.Inventory.View;

namespace Infrustructure.Services.MVPWindows.Examples.Inventory.Presenter
{
    public sealed class InventoryWindowPresenter
        : BaseWindowPresenter<InventoryWindowView, InventoryWindowModel, InventoryWindowArgs>
    {
        private readonly InventoryListPresenter _listPresenter;
        private readonly InventoryDetailsPresenter _detailsPresenter;

        public InventoryWindowPresenter(
            InventoryWindowView view,
            InventoryWindowModel model,
            IWindowsManager windowsManager,
            InventoryListPresenter listPresenter,
            InventoryDetailsPresenter detailsPresenter)
            : base(view, model, windowsManager)
        {
            _listPresenter = listPresenter;
            _detailsPresenter = detailsPresenter;

            _listPresenter.OpenItemRequested += OnOpenItemRequested;
            _listPresenter.LockedItemClicked += OnLockedItemClicked;
            _detailsPresenter.BackRequested += OnBackRequested;
        }

        public override void Dispose()
        {
            _listPresenter.OpenItemRequested -= OnOpenItemRequested;
            _listPresenter.LockedItemClicked -= OnLockedItemClicked;
            _detailsPresenter.BackRequested -= OnBackRequested;
            _listPresenter.Dispose();
            _detailsPresenter.Dispose();

            base.Dispose();
        }

        protected override void BeforeOpen() =>
            View.PrepareForOpen();

        protected override async UniTask AfterOpenAsync()
        {
            _listPresenter.ShowItems(Model.Items);
            
            await View.ShowListAsync();
        }

        private void OnOpenItemRequested(InventoryItemData item)
        {
            Model.SelectItem(item);
            
            ShowDetailsFlowAsync(item).Forget();
        }

        private async UniTask ShowDetailsFlowAsync(InventoryItemData item)
        {
            await View.HideListAsync();
            await _detailsPresenter.ShowAsync(item);
        }

        private async UniTask BackToListFlowAsync()
        {
            await _detailsPresenter.HideAsync();
            await View.ShowListAsync();
        }

        private void OnLockedItemClicked(InventoryItemData item) =>
            UnityEngine.Debug.Log($"Inventory item is locked: {item.Title}");

        private void OnBackRequested() =>
            BackToListFlowAsync().Forget();
    }
}
