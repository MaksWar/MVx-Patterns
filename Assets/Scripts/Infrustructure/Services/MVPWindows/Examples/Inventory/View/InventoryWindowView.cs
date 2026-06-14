using Cysharp.Threading.Tasks;
using Infrustructure.Services.MVPWindows.Examples.Inventory.Model;

namespace Infrustructure.Services.MVPWindows.Examples.Inventory.View
{
    public sealed class InventoryWindowView : BaseWindowView
    {
        public const string PrefabName = "InventoryWindow";

        [UnityEngine.SerializeField] private InventoryListView listView;
        [UnityEngine.SerializeField] private InventoryDetailsView detailsView;

        public InventoryListView ListView => listView;
        public InventoryDetailsView DetailsView => detailsView;

        public override UniTask InitializeAsync()
        {
            detailsView.Initialize();

            return base.InitializeAsync();
        }

        public void PrepareForOpen()
        {
            listView.HideInstant();
            detailsView.HideInstant();
        }

        public UniTask ShowListAsync() =>
            listView.PlayShowAsync();

        public async UniTask HideListAsync() =>
            await listView.PlayHideAsync();

        public UniTask ShowDetailsAsync(InventoryItemData item) =>
            detailsView.PlayShowAsync(item);

        public async UniTask HideDetailsAsync() =>
            await detailsView.PlayHideAsync();
    }
}
