using System;
using UnityEngine;

namespace Infrustructure.Services.MVPWindows.Examples.Inventory.Model
{
    [Serializable]
    public sealed class InventoryItemData
    {
        public string Id;
        public string Title;
        public string Description;
        public int Amount;
        public InventoryItemState State = InventoryItemState.Open;
        public Sprite Icon;
        public Color RarityColor = Color.white;
    }

    public enum InventoryItemState
    {
        Open = 0,
        Locked = 1
    }
}
