using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Infrustructure.Services.MVPWindows.Examples.Inventory.Model;
using Infrustructure.Services.MVPWindows.Examples.Inventory.View;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Infrustructure.Services.MVPWindows.Examples.Inventory.Caller
{
    public sealed class InventoryWindowCaller : MonoBehaviour
    {
        [SerializeField] private Button openInventoryButton;
        [SerializeField] private List<InventoryItemData> demoItems = new();

        private IWindowsManager _windowsManager;

        [Inject]
        private void Construct(IWindowsManager windowsManager) =>
            _windowsManager = windowsManager;

        private void Awake() =>
            openInventoryButton.onClick.AddListener(OpenInventory);

        private void OpenInventory() =>
            OpenInventoryAsync().Forget();

        private async UniTask OpenInventoryAsync()
        {
            var args = new InventoryWindowArgs
            {
                Items = CreateItems()
            };

            var options = new WindowOptions
            {
                OnOpened = () => Debug.Log("Inventory opened"),
                OnClosed = () => Debug.Log("Inventory closed")
            };

            await _windowsManager.OpenWindowAsync(InventoryWindowView.PrefabName, args, options);
        }

        private List<InventoryItemData> CreateItems()
        {
            if (demoItems.Count > 0)
            {
                return new List<InventoryItemData>(demoItems);
            }

            return new List<InventoryItemData>
            {
                new()
                {
                    Id = "sword_01",
                    Title = "Iron Sword",
                    Description = "Reliable blade for close combat.",
                    Amount = 1,
                    RarityColor = Color.gray
                },
                new()
                {
                    Id = "potion_01",
                    Title = "Health Potion",
                    Description = "Restores a small amount of health.",
                    Amount = 5,
                    State = InventoryItemState.Open,
                    RarityColor = Color.green
                },
                new()
                {
                    Id = "gem_01",
                    Title = "Ancient Gem",
                    Description = "Locked until the ancient vault is opened.",
                    Amount = 2,
                    State = InventoryItemState.Locked,
                    RarityColor = Color.cyan
                }
            };
        }
    }
}
