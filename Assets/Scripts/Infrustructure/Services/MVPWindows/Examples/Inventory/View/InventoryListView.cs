using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Infrustructure.Services.MVPWindows.Examples.Inventory.Model;
using Infrustructure.Services.Windows.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace Infrustructure.Services.MVPWindows.Examples.Inventory.View
{
    public sealed class InventoryListView : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform contentRoot;
        [SerializeField] private InventoryItemView itemPrefab;
        [SerializeField] private BaseWindowAnimationComponent[] animationComponents;

        private readonly List<InventoryItemView> _items = new();

        public event Action<InventoryItemData> ItemClicked;

        public void Bind(IReadOnlyList<InventoryItemData> items)
        {
            ClearItems();

            foreach (InventoryItemData item in items)
            {
                InventoryItemView itemView = Instantiate(itemPrefab, contentRoot);
                itemView.Initialize();
                itemView.Bind(item);
                itemView.Clicked += OnItemClicked;
                
                _items.Add(itemView);
            }

            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        public async UniTask PlayHideAsync()
        {
            await PlayCloseAsync();
            
            gameObject.SetActive(false);
        }

        public UniTask PlayShowAsync() =>
            PlayOpenAsync();

        public void HideInstant() =>
            gameObject.SetActive(false);

        private async UniTask PlayOpenAsync()
        {
            gameObject.SetActive(true);

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
            if (animationComponents != null)
            {
                foreach (BaseWindowAnimationComponent animationComponent in animationComponents)
                {
                    if (animationComponent != null)
                    {
                        tasks.Add(animationComponent.CloseAsync());
                    }
                }
            }

            await UniTask.WhenAll(tasks);
        }

        private void ClearItems()
        {
            foreach (InventoryItemView itemView in _items)
            {
                if (itemView != null)
                {
                    itemView.Clicked -= OnItemClicked;
                    Destroy(itemView.gameObject);
                }
            }

            _items.Clear();
        }

        private void OnItemClicked(InventoryItemData item) =>
            ItemClicked?.Invoke(item);
    }
}
