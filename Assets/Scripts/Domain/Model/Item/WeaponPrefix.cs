#nullable enable

using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace Domain.Model.Item
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/WeaponPrefix")]
    public class WeaponPrefix : ScriptableObject, IHasRarity
    {
        [ReadOnly] [Required] public string Name;
        [SerializeField] private Rarity _rarity;
        public Rarity Rarity => _rarity;
        [MinValue(0)] public float PowerMagnification = 1;
        [MinValue(0)] public int FeatureLimitAdditional = 0;
        [MinValue(0)] public float UsageLimitMagnification = 1;
        public int AdditionalUpgradeLimit;
        public bool IsCursed;
#if UNITY_EDITOR
        private void OnValidate()
        {
            var assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
            Name = Path.GetFileNameWithoutExtension(assetPath);
            EditorUtility.SetDirty(this);
        }
#endif
    }
}