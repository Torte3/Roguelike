#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character.Data;
using Domain.Model.Condition;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Effect.Skill;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Item.Data;
using Domain.Model.Item.Description;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.WorldEvents;
using Domain.Service.Effect;
using R3;
using UnityEngine;
using Utilities;
using Utilities.Result;
using Utilities.Serialize.Option;

namespace Domain.Service.Items
{
    public sealed class EquipmentItem : BaseItem, IEquipmentToggleTarget, ISerializable<EquipmentItemMemento>, IEquipmentMergeBase
    {
        private readonly ReactiveProperty<bool> _equippedState;
        private readonly List<ArtifactPassiveConditionBundle> _passiveConditionSlots;

        public int SlotLimit { get; }


        public override string RevealedName => BaseName;
        public override ItemCategory Category => ItemCategory.Artifacts;
        protected override bool HasSameEffect => false;
        protected override bool HasSameSkill => false;
        public override bool UseOnDeath => false;
        public override bool RequiresLiteracy => false;
        public override bool IdentifyIfGot => false;
        public override bool IdentifyIfUsed => false;
        public override bool AutoDestroyWhenDisabled => false;

        public EquipmentItem(ArtifactData data) : this(Build(data))
        {
        }

        public EquipmentItem(EquipmentItemMemento data) : base(data.BaseItem)
        {
            _passiveConditionSlots = data.PassiveConditionSlots;
            SlotLimit = data.SlotLimit;

            _equippedState = new ReactiveProperty<bool>(data.IsEquipped);
            _equippedState.AddTo(_disposables);
        }

        public override Option<bool> IsEquipped => Option.Some(_equippedState.CurrentValue);

        public override ReadOnlyReactiveProperty<bool> IsPassiveActive => _equippedState;

        public override bool CanActivateWhenUsed =>
            HasUsableSkillOnUse() && !(IsCursed && _equippedState.CurrentValue);

        public override bool CanActivateWhenThrown => HasUsableSkillOnThrow();

        public override bool CanAttemptUse => HasUsableSkillOnUse();

        public override bool CanAttemptThrow => !IsDiscardBlocked;

        public override bool IsDiscardBlocked =>
            IsCursed && _equippedState.CurrentValue;

        public override Option<ISkillWithCost> SkillOnUse { get; } = Option.Some(
            (ISkillWithCost)new SkillWithCost(
                new SkillWithCostMemento(
                    EquipToggleSkill.BuildMemento(),
                    cost: 0,
                    chargeTurn: 0,
                    coolTime: 0,
                    remainingTurn: 0)));
        public override Option<ISkillWithCost> SkillOnThrow => Option.None<ISkillWithCost>();

        public override Result<Unit, ItemActionFailure> ActivationCheckWhenUsed()
        {
            return UnequipCheck();
        }

        private Result<Unit, ItemActionFailure> UnequipCheck()
        {
            return Result<Unit, ItemActionFailure>.OkIf(!(IsCursed && _equippedState.CurrentValue), Unit.Default,
                ItemActionFailure.Cursed);
        }

        public bool TryToggleEquipped(IActorOfEffect actor, IMap map)
        {
            if (UnequipCheck().IsFailed(out var failure))
            {
                map.Events.Record(new ItemActionFailed(actor.Entity.IsVisible, this.NameIn(map), EquipState, ItemAction.Use, failure));
                return false;
            }

            var nameBefore = this.NameIn(map);
            _equippedState.Value = !_equippedState.CurrentValue;
            RecordChange(actor, map, _equippedState.CurrentValue ? ItemChangeKind.Equipped : ItemChangeKind.Unequipped, nameBefore);
            return true;
        }

        public void ForceUnequip()
        {
            if (!_equippedState.CurrentValue)
                return;

            _equippedState.Value = false;
        }

        public override void Repair(IEntity itemHolder, IMap map)
        {
        }

        private protected override void FinishUse(IActorOfEffect actor, IMap map, bool isConsumed)
        {
            var nameBefore = this.NameIn(map);
            if (State == ItemState.ShopItem)
            {
                SetState(ItemState.UsedShopItem);
            }

            RecordChange(actor, map, ItemChangeKind.Used, nameBefore);
        }

        public EquipmentItemMemento Serialize()
        {
            var json = JsonUtility.ToJson(new EquipmentItemMemento(
                SerializeBase(),
                _passiveConditionSlots,
                SlotLimit,
                IsEquipped.UnwrapOr(false)));
            return JsonUtility.FromJson<EquipmentItemMemento>(json);
        }

        public static EquipmentItemMemento Build(
            ArtifactData data,
            bool isCursed = false,
            ItemState state = ItemState.None,
            EnemyData? mimic = null,
            bool isEquipped = false)
        {
            var hasBuiltIn = data.HasBuiltInPassive;
            var slots = new List<ArtifactPassiveConditionBundle>();
            if (hasBuiltIn)
            {
                slots.Add(data.BuiltInPassiveConditionBundle.Clone());
            }

            var slotLimit = (hasBuiltIn ? 1 : 0) + data.SynthesisSlotLimit;

            var conditions = FlattenConditionList(slots);
            var json = JsonUtility.ToJson(new EquipmentItemMemento(
                BuildBase(
                    baseName: data.name,
                    icon: data.Icon,
                    isShiny: data.IsShiny,
                    rarity: data.Rarity,
                    customBasePrice: data.UseCustomBasePrice ? data.CustomBasePrice : null,
                    additionalPrice: data.AdditionalPrice,
                    multiplyPrice: data.MultiplyPrice,
                    state: state,
                    upgradeCount: 0,
                    maxUsages: 0,
                    usageLossChance: 1f,
                    isCursed: isCursed,
                    upgradeLimit: 0,
                    conditions: conditions,
                    mimic: mimic.ToOption()),
                slots,
                slotLimit,
                isEquipped));
            return JsonUtility.FromJson<EquipmentItemMemento>(json);
        }

        public override bool CanAcceptMergeMaterial(IItem material) => material.CanBeMergedInto(this);

        public override IItem MergeWith(IItem material) => material.MergeInto(this);

        public override bool CanBeMergedInto(IEquipmentMergeBase equipment) =>
            equipment.CanTakePassiveSlots(_passiveConditionSlots);

        public override IItem MergeInto(IEquipmentMergeBase equipment) => equipment.TakePassiveSlots(_passiveConditionSlots);

        public bool CanTakePassiveSlots(IReadOnlyList<ArtifactPassiveConditionBundle> slots)
        {
            return _passiveConditionSlots.Count < SlotLimit && slots.Count > 0;
        }

        public IItem TakePassiveSlots(IReadOnlyList<ArtifactPassiveConditionBundle> materialSlots)
        {
            var memento = Serialize();
            var newSlots = memento.PassiveConditionSlots.Select(b => b.Clone()).ToList();
            foreach (var bundle in materialSlots)
            {
                if (newSlots.Count >= memento.SlotLimit)
                    break;
                newSlots.Add(bundle.Clone());
            }

            var conditions = FlattenConditionList(newSlots);
            return new EquipmentItem(memento.CopyWith(
                baseItem: memento.BaseItem.CopyWith(conditions: conditions),
                passiveConditionSlots: newSlots));
        }

        private static List<IConditionData> FlattenConditionList(IReadOnlyList<ArtifactPassiveConditionBundle> slots)
        {
            var list = new List<IConditionData>();
            foreach (var bundle in slots)
            {
                foreach (var c in bundle.Conditions)
                    list.Add(c);
            }

            return list;
        }

        public override bool CanUpgrade() => false;
        public override bool CanDowngrade() => false;

        public override void Upgrade(IEntity itemHolder, IMap map) =>
            throw new Exception("Cannot upgrade equipment item");

        public override void Downgrade(IEntity itemHolder, IMap map) =>
            throw new Exception("Cannot downgrade equipment item");

        public override IReadOnlyItem PreviewUpgrade() =>
            throw new Exception("Cannot upgrade equipment item");

        protected override string? BuildTemplatedActivatableSkillInfo() => null;

        protected override ItemAbilities? Abilities =>
            new(ItemAbilityKind.PassiveSkills, SlotLimit,
                _passiveConditionSlots.Select(bundle => bundle.DisplayName).ToList());
    }
}
