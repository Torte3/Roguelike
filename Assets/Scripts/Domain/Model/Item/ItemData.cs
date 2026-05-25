#nullable enable

using System.Collections.Generic;
using Domain.Model.Condition;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Evaluation;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Linq;
using Utilities.Validation;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Domain.Model.Item
{
    public interface IItemData : IHasRarity
    {
    }
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Item")]
    public class ItemData : ScriptableObject, IItemData
    {
        [SerializeField] public ItemCategory Category;
        [Required] public Sprite Icon;
        public bool IsShiny;
        [SerializeField] private Rarity _rarity;
        public Rarity Rarity => _rarity;
        private bool IsNotOthersCategory() => Category != ItemCategory.Others;
        [ShowIf(nameof(IsNotOthersCategory))]
        [SerializeField] private bool _useCustomBasePrice = false;
        public bool UseCustomBasePrice => Category == ItemCategory.Others || _useCustomBasePrice;
        [ShowIf(nameof(UseCustomBasePrice))]
        [MinValue(0)]
        public int CustomBasePrice = 0;
        [ShowIf("@!" + nameof(UseCustomBasePrice))]
        public int AdditionalPrice = 0;
        [ShowIf("@!" + nameof(UseCustomBasePrice))]
        public float MultiplyPrice = 1f;
        public ItemEffectType EffectType = ItemEffectType.SpawnEffect;

        private bool IsSpawnEffect() => EffectType == ItemEffectType.SpawnEffect;
        private bool IsItemTarget() => EffectType == ItemEffectType.ItemTarget;
        private bool IsInventoryTarget() => EffectType == ItemEffectType.InventoryTarget;

        #region spawn effect
        [ShowIf(nameof(SpawnEffectsOnUse))] public bool UseOnDeath;

        [ShowIf(nameof(IsSpawnEffect))]
        public bool SpawnEffectsOnUse = true;

        [ShowIf(nameof(IsSpawnEffect))]
        public bool SpawnEffectsOnThrow;

        [ShowIf("@" + nameof(SpawnEffectsOnUse) + " && " + nameof(SpawnEffectsOnThrow) + " && !" + nameof(IsSameSkill))]
        public bool IsSameEffect;

        [ShowIf("@" + nameof(SpawnEffectsOnUse) + " && " + nameof(SpawnEffectsOnThrow))]
        public bool IsSameSkill;

        [ShowIf(nameof(SpawnEffectsOnUse))] public SkillDataOnUse? SkillOnUse;
        [ShowIf(nameof(SpawnEffectsOnThrow))] public SkillDataOnThrow? SkillOnThrow;
        #endregion

        #region item target
        [ShowIf(nameof(IsItemTarget))]
        [SerializeReference]
        [RequiredIfShown]
        public IItemEffect? ItemEffect;
        #endregion

        #region inventory target
        [ShowIf(nameof(IsInventoryTarget))]
        [SerializeReference]
        [RequiredIfShown]
        public IInventoryEffect? InventoryEffect;
        #endregion

        [ShowIf(nameof(_usable))][MinValue(1)] public int UsageLimit;
        public int UpgradeLimit = 3;
        [SerializeReference] public List<IConditionData> PassiveConditions;
        [SerializeField] private List<StringSerializableItemFeature> _featuresToMergeWeapon;
        public List<ItemFeature> FeaturesToMergeWeapon => _featuresToMergeWeapon.Select(feature => feature.Value).ToList();


        private bool _usable => EffectType switch
        {
            ItemEffectType.SpawnEffect => SpawnEffectsOnUse || SpawnEffectsOnThrow,
            ItemEffectType.ItemTarget => ItemEffect != null,
            ItemEffectType.InventoryTarget => InventoryEffect != null,
            _ => false
        };

        public void AddEffects(List<IEffect> effects)
        {
            if (SkillOnUse != null)
            {
                SkillOnUse.Effects.AddRange(effects);
            }

            if (SkillOnThrow != null)
            {
                SkillOnThrow.Effects.AddRange(effects);
            }
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            switch (EffectType)
            {
                case ItemEffectType.None:
                    SpawnEffectsOnUse = false;
                    SpawnEffectsOnThrow = false;
                    ItemEffect = null;
                    break;
                case ItemEffectType.SpawnEffect:
                    ItemEffect = null;
                    break;
                case ItemEffectType.ItemTarget:
                    SpawnEffectsOnUse = false;
                    SpawnEffectsOnThrow = false;
                    break;
            }

            if (!SpawnEffectsOnUse)
            {
                UseOnDeath = false;
            }

            if (!(SpawnEffectsOnUse && SpawnEffectsOnThrow))
            {
                IsSameEffect = false;
                IsSameSkill = false;
            }

            if (IsSameEffect && SkillOnUse != null && SkillOnThrow != null)
            {
                SkillOnThrow.SetSameEffect(SkillOnUse);
            }

            if (IsSameSkill && SkillOnUse != null)
            {
                if (SkillOnThrow == null)
                {
                    SkillOnThrow =
                        new SkillDataOnThrow(SkillOnUse.Area, SkillOnUse.Effects,
                            CommonSenseParameters.SkillOnThrowProbabilityOfSuccess);
                }
                else
                {
                    SkillOnThrow =
                        new SkillDataOnThrow(SkillOnUse.Area, SkillOnUse.Effects, SkillOnThrow.ProbabilityOfSuccess);
                }
            }

            if (SkillOnUse != null)
            {
                SkillOnUse.OnValidate();
            }

            if (SkillOnThrow != null)
            {
                SkillOnThrow.OnValidate();
            }

            EditorUtility.SetDirty(this);
        }
#endif
    }
}