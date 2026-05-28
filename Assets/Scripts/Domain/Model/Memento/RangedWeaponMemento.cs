#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.Item;
using UnityEngine;
using Utilities.Serialize;
using Utilities.Serialize.Option;

namespace Domain.Model.Memento
{
    [Serializable]
    public class RangedWeaponMemento : IItemMemento
    {
        [field: SerializeField] public BaseItemMemento BaseItem { get; private set; }
        [SerializeField] private Option<ScriptableObjectSerializable<WeaponPrefix>> _prefix;
        public Option<WeaponPrefix> Prefix => _prefix.Map(p => p.Value);
        [field: SerializeField] public int DefaultPower { get; private set; }
        [field: SerializeField] public IconSerializable ProjectileIcon { get; private set; }
        [field: SerializeField] public List<ItemFeature> Features { get; private set; }
        [field: SerializeField] public int FeatureLimit { get; private set; }
        [field: SerializeField] public SkillWithCostMemento SkillOnUse { get; private set; }

        public RangedWeaponMemento(
            BaseItemMemento baseItem,
            Option<WeaponPrefix> prefix,
            int defaultPower,
            IconSerializable projectileIcon,
            List<ItemFeature> features,
            int featureLimit,
            SkillWithCostMemento skillOnUse
        )
        {
            BaseItem = baseItem;
            _prefix = prefix.Map(p => p.ToSerializable());
            DefaultPower = defaultPower;
            ProjectileIcon = projectileIcon;
            Features = features;
            FeatureLimit = featureLimit;
            SkillOnUse = skillOnUse;
        }

        public RangedWeaponMemento CopyWith(
            BaseItemMemento? baseItem = null,
            Option<WeaponPrefix>? prefix = null,
            int? defaultPower = null,
            IconSerializable? projectileIcon = null,
            List<ItemFeature>? features = null,
            int? featureLimit = null,
            SkillWithCostMemento? skillOnUse = null
        )
        {
            return new RangedWeaponMemento(
                baseItem ?? BaseItem,
                prefix ?? Prefix,
                defaultPower ?? DefaultPower,
                projectileIcon ?? ProjectileIcon,
                features ?? Features,
                featureLimit ?? FeatureLimit,
                skillOnUse ?? SkillOnUse
            );
        }
    }
}