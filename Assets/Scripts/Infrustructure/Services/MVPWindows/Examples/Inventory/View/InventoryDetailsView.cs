using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Infrustructure.Services.MVPWindows.Examples.Inventory.Model;
using Infrustructure.Services.Windows.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace Infrustructure.Services.MVPWindows.Examples.Inventory.View
{
    public sealed class InventoryDetailsView : MonoBehaviour
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text amountText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image rarityImage;
        [SerializeField] private BaseWindowAnimationComponent[] animationComponents;

        public event Action CloseClicked;

        public void Initialize() =>
            closeButton.onClick.AddListener(OnCloseClicked);

        public async UniTask PlayShowAsync(InventoryItemData item)
        {
            Bind(item);

            gameObject.SetActive(true);

            await PlayOpenAsync();
        }

        public async UniTask PlayHideAsync()
        {
            await PlayCloseAsync();

            gameObject.SetActive(false);
        }

        public void HideInstant() =>
            gameObject.SetActive(false);

        private void Bind(InventoryItemData item)
        {
            titleText.text = item.Title;
            descriptionText.text = item.Description;
            amountText.text = item.Amount.ToString();

            iconImage.sprite = item.Icon;
            iconImage.enabled = item.Icon != null;
            
            rarityImage.color = item.RarityColor;
        }

        private async UniTask PlayOpenAsync()
        {
            var tasks = new List<UniTask>();
            if (animationComponents != null)
            {
                foreach (BaseWindowAnimationComponent animationComponent in animationComponents)
                {
                    if (animationComponent != null)
                    {
                        tasks.Add(animationComponent.OpenAsync());
                    }
                }
            }

            await UniTask.WhenAll(tasks);
        }

        private async UniTask PlayCloseAsync()
        {
            var tasks = new List<UniTask>();
            foreach (BaseWindowAnimationComponent animationComponent in animationComponents)
            {
                if (animationComponent != null)
                {
                    tasks.Add(animationComponent.CloseAsync());
                }
            }

            await UniTask.WhenAll(tasks);
        }

        private void OnCloseClicked() =>
            CloseClicked?.Invoke();
    }
}