using Cysharp.Threading.Tasks;
using Infrustructure.AssetManagement;
using UnityEngine;
using Zenject;

namespace Infrustructure.Services.UI
{
    public sealed class UIRootInitializer : IUIRootInitializer
    {
        private readonly DiContainer _diContainer;
        private readonly IAssetsProvider _assetProvider;

        public UIRootInitializer(DiContainer diContainer, IAssetsProvider assetProvider)
        {
            _diContainer = diContainer;
            _assetProvider = assetProvider;
        }

        public async UniTask InitializeAsync()
        {
            GameObject guiControllerPrefab = await _assetProvider.Load<GameObject>(GetPrefabName("GUI"), GetType());
            GameObject guiOverHudControllerPrefab = await _assetProvider.Load<GameObject>(GetPrefabName("GUIOverHUD"), GetType());

            GameObject guiInstance = _diContainer.InstantiatePrefab(guiControllerPrefab);
            guiInstance.name = "GUI";

            GameObject guiOverHudInstance = _diContainer.InstantiatePrefab(guiOverHudControllerPrefab);
            guiOverHudInstance.name = "GUIOverHUD";
        }

        private static string GetPrefabName(string prefabName) =>
            $"Windows/{prefabName}.prefab";
    }
}
