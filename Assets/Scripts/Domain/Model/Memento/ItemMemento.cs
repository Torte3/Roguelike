#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.Dungeon;
using Domain.Model.Item;
using UnityEngine;
using Utilities.Serialize.Option;

namespace Domain.Model.Memento
{

    [Serializable]
    public class ItemMemento : IItemMemento
    {
        [field: SerializeField] public BaseItemMemento BaseItem { get; private set; }
        [field: SerializeField] public ItemCategory Category { get; private set; }
        [field: SerializeField] public Option<SkillWithCostMemento> SkillOnUse { get; private set; }
        [field: SerializeField] public Option<SkillWithCostMemento> SkillOnThrow { get; private set; }
        [field: SerializeField] public bool HasSameEffect { get; private set; }
        [field: SerializeField] public bool HasSameSkill { get; private set; }
        [field: SerializeField] public bool UseOnDeath { get; private set; }
        [field: SerializeField] public List<ItemFeature> FeaturesToMergeWeapon { get; private set; }

        public ItemMemento(
            BaseItemMemento baseItem,
            ItemCategory category,
            Option<SkillWithCostMemento> skillOnUse,
            Option<SkillWithCostMemento> skillOnThrow,
            bool hasSameEffect,
            bool hasSameSkill,
            bool useOnDeath,
            List<ItemFeature> featuresToMergeWeapon)
        {
            BaseItem = baseItem;
            Category = category;
            SkillOnUse = skillOnUse;
            SkillOnThrow = skillOnThrow;
            HasSameEffect = hasSameEffect;
            HasSameSkill = hasSameSkill;
            UseOnDeath = useOnDeath;
            FeaturesToMergeWeapon = featuresToMergeWeapon;
        }

        public ItemMemento CopyWith(
            BaseItemMemento? baseItem = null,
            ItemCategory? category = null,
            Option<SkillWithCostMemento>? skillOnUse = null,
            Option<SkillWithCostMemento>? skillOnThrow = null,
            bool? hasSameEffect = null,
            bool? hasSameSkill = null,
            bool? useOnDeath = null,
            List<ItemFeature>? featuresToMergeWeapon = null)
        {
            return new ItemMemento(
                baseItem ?? BaseItem,
                category ?? Category,
                skillOnUse ?? SkillOnUse,
                skillOnThrow ?? SkillOnThrow,
                hasSameEffect ?? HasSameEffect,
                hasSameSkill ?? HasSameSkill,
                useOnDeath ?? UseOnDeath,
                featuresToMergeWeapon ?? FeaturesToMergeWeapon
            );
        }
    }
}