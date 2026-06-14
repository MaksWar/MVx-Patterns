using System;
using Infrustructure.Services.MVPWindows.Examples.Inventory.Model;
using UnityEngine;
using UnityEngine.UI;

namespace Infrustructure.Services.MVPWindows.Examples.Inventory.View
{
    public sealed class InventoryItemView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Text titleText;
        [SerializeField] private Text amountText;
        [SerializeField] private Text stateText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image rarityImage;
        [SerializeField] private CanvasGroup canvasGroup;

        private InventoryItemData _item;

        public event Action<InventoryItemData> Clicked;

        public void Initialize() =>
            button.onClick.AddListener(OnClicked);

        public void Bind(InventoryItemData item)
        {
            _item = item;

            titleText.text = item.Title;
            amountText.text = item.Amount.ToString();

            bool isLocked = item.State == InventoryItemState.Locked;
            stateText.text = isLocked ? "Locked" : string.Empty;

            iconImage.sprite = item.Icon;
            iconImage.enabled = item.Icon != null;

            rarityImage.color = item.RarityColor;
            canvasGroup.alpha = isLocked ? 0.55f : 1f;
        }

        private void OnClicked() =>
            Clicked?.Invoke(_item);
    }
}
