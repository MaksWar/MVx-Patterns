using System.Collections.Generic;

namespace Infrustructure.Services.MVPWindows.Examples.Inventory.Model
{
    public sealed class InventoryWindowModel : BaseWindowModel<InventoryWindowArgs>
    {
        public IReadOnlyList<InventoryItemData> Items => Args.Items;
        public InventoryItemData SelectedItem { get; private set; }

        public override void Initialize(InventoryWindowArgs args, WindowOptions options)
        {
            base.Initialize(args, options);

            SelectedItem = null;
        }

        public void SelectItem(InventoryItemData item) =>
            SelectedItem = item;
    }
}
