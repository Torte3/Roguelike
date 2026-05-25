#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.Item;
using UnityEngine;
using Utilities.Serialize.Option;

namespace Domain.Model.Memento
{
    [Serializable]
    public class DirectWeaponMemento : IItemMemento
    {
        [field: SerializeField] public BaseItemMemento BaseItem { get; private set; }
        [field: SerializeField] public Option<WeaponPrefix> Prefix { get; private set; }
        [field: SerializeField] public int DefaultPower { get; private set; }
        [field: SerializeField] public List<ItemFeature> Features { get; private set; }
        [field: SerializeField] public int FeatureLimit { get; private set; }
        [field: SerializeField] public SkillWithCostMemento SkillOnUse { get; private set; }
        [field: SerializeField] public SkillWithCostMemento SkillOnThrow { get; private set; }
        [field: SerializeField] public bool HasSameEffect { get; private set; }

        public DirectWeaponMemento(
            BaseItemMemento baseItem,
            Option<WeaponPrefix> prefix,
            int defaultPower,
            List<ItemFeature> features,
            int featureLimit,
            SkillWithCostMemento skillOnUse,
            SkillWithCostMemento skillOnThrow,
            bool hasSameEffect
        )
        {
            BaseItem = baseItem;
            Prefix = prefix;
            DefaultPower = defaultPower;
            Features = features;
            FeatureLimit = featureLimit;
            SkillOnUse = skillOnUse;
            SkillOnThrow = skillOnThrow;
            HasSameEffect = hasSameEffect;
        }

        public DirectWeaponMemento CopyWith(
            BaseItemMemento? baseItem = null,
            Option<WeaponPrefix>? prefix = null,
            int? defaultPower = null,
            List<ItemFeature>? features = null,
            int? featureLimit = null,
            SkillWithCostMemento? skillOnUse = null,
            SkillWithCostMemento? skillOnThrow = null,
            bool? hasSameEffect = null
        )
        {
            return new DirectWeaponMemento(
                baseItem ?? BaseItem,
                prefix ?? Prefix,
                defaultPower ?? DefaultPower,
                features ?? Features,
                featureLimit ?? FeatureLimit,
                skillOnUse ?? SkillOnUse,
                skillOnThrow ?? SkillOnThrow,
                hasSameEffect ?? HasSameEffect
            );
        }
    }
}