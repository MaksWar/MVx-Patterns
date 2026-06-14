using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Infrustructure.AssetManagement
{
    public class PrefabFactoryAsync<TComponent> : IFactory<string, UniTask<TComponent>>
    {
        private readonly IInstantiator _instantiator;
        private readonly IAssetsProvider _assetProvider;

        public PrefabFactoryAsync(IInstantiator instantiator, IAssetsProvider assetProvider)
        {
            _instantiator = instantiator;
            _assetProvider = assetProvider;
        }

        public async UniTask<TComponent> Create(string assetKey)
        {
            GameObject prefab = await _assetProvider.Load<GameObject>(assetKey, GetType());
            GameObject newObject = _instantiator.InstantiatePrefab(prefab);

            return newObject.GetComponent<TComponent>();
        }
    }
}