#nullable enable
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
        public override bool CanDowngrade() => UpgradeCount > 0;
    }
}
