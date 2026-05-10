#nullable enable

using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model.Item
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Artifact")]
    public class ArtifactData : ScriptableObject, IItemData
    {
        [Required] public Sprite Icon = null!;
        public bool IsShiny;
        [SerializeField] private Rarity _rarity;
        public Rarity Rarity => _rarity;
        public bool UseCustomBasePrice;
        [ShowIf(nameof(UseCustomBasePrice))]
        [MinValue(0)]
        public int CustomBasePrice;
        [MinValue(0)] public int AdditionalPrice;
        public float MultiplyPrice = 1f;

        [MinValue(0)]
        public int SynthesisSlotLimit;

        public bool HasBuiltInPassive;

        [ShowIf(nameof(HasBuiltInPassive))]
        [Required]
        public ArtifactPassiveConditionBundle BuiltInPassiveConditionBundle;
    }
}
