using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Utilities
{
    public static class ObjectLoader
    {
        public static T Load<T>(string name) where T : ScriptableObject
        {
            return Addressables.LoadAssetAsync<T>($"Assets/Database/{typeof(T).Name}/{name}.asset")
                        .WaitForCompletion();
        }
        public static T LoadWithPath<T>(string path)
        {
            return Addressables.LoadAssetAsync<T>(path).WaitForCompletion();
        }
        public static GameObject LoadPrefab(string name)
        {
            return Addressables.LoadAssetAsync<GameObject>($"Assets/Prefabs/{name}.prefab").WaitForCompletion();
        }
        public static GameObject LoadParticle(string name)
        {
            return Addressables.LoadAssetAsync<GameObject>($"EffectPrefabs/{name}.prefab").WaitForCompletion();
        }
        public static Sprite LoadIcon(string name)
        {
            return Addressables.LoadAssetAsync<Sprite>($"Assets/Images/icons_full_16.png[{name}]").WaitForCompletion();
        }
        public static Sprite LoadMapChip(string name)
        {
            return Addressables.LoadAssetAsync<Sprite>($"MapChip/(Base)BaseChip_pipo.png[{name}]").WaitForCompletion();
        }
        public static RuntimeAnimatorController LoadAnimation(string name)
        {
            return Addressables.LoadAssetAsync<RuntimeAnimatorController>($"Assets/Animations/{name}.controller")
                .WaitForCompletion();
        }
    }
}