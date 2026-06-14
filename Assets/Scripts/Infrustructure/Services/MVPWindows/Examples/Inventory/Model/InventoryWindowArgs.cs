using System.Collections.Generic;

namespace Infrustructure.Services.MVPWindows.Examples.Inventory.Model
{
    public sealed class InventoryWindowArgs : WindowArgs
    {
        public List<InventoryItemData> Items = new();
    }
}
