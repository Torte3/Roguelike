#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Dungeon;
using Domain.Model.Item;
using Domain.Model.Memento;
using Utilities.Serialize.Option;

namespace Domain.Service.Items
{
    public abstract class WeaponConsumableItem : ConsumableItem
    {
        private protected bool _hasSameEffect;

        protected WeaponConsumableItem(BaseItemMemento baseItem, Option<WeaponPrefix> prefix) : base(baseItem)
        {
            WeaponPrefix = prefix;
        }

        protected Option<WeaponPrefix> WeaponPrefix { get; private set; }

        public override ItemCurseKind CurseKind => ItemCurseKind.CannotDiscardWhenCursed;

        public override string RevealedName => WeaponPrefix.MapOr("", p => p.Name) + BaseName;
        public override ItemCategory Category => ItemCategory.Weapons;
        protected override bool HasSameEffect => _hasSameEffect;
        protected override bool HasSameSkill => false;
        public override bool UseOnDeath => false;
        public override bool RequiresLiteracy => false;
        public override bool IdentifyIfGot => true;
        public override bool IdentifyIfUsed => true;
        public override bool AutoDestroyWhenDisabled => false;

        public override bool CanUpgrade() => UpgradeCount < UpgradeLimit;

        private protected abstract IReadOnlyList<ItemFeature> WeaponFeatures { get; }
        private protected abstract int WeaponFeatureLimit { get; }
        private protected abstract FeatureApplicabilityTag WeaponApplicability { get; }
        private protected abstract IItem MergeWeapon(IEnumerable<ItemFeature> featuresToMergeWeapon, int additionalUpgrade);

        public override IReadOnlyList<ItemFeature>? FeaturesForWeaponMerge => WeaponFeatures;

        public override bool CanAcceptMergeMaterial(IItem material)
        {
            var features = WeaponFeatures;
            var materialFeatures = material.FeaturesForWeaponMerge;
            if (materialFeatures == null)
                return false;
            if (!features.Merge(materialFeatures, WeaponFeatureLimit, WeaponApplicability).SequenceEqual(features))
                return true;
            return material.UpgradeCount > 0 && CanUpgrade();
        }

        public override IItem MergeWith(IItem material)
        {
            var materialFeatures = material.FeaturesForWeaponMerge
                ?? throw new ArgumentException("Invalid merge target: only another weapon or an item with mergeable weapon features is allowed.");
            return MergeWeapon(materialFeatures, material.UpgradeCount);
        }

        public override bool CanDowngrade() => UpgradeCount > 0;
    }
}
