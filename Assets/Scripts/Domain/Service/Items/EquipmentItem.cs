#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Condition;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Effect;
using Domain.Service.Logs;
using R3;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Service.Items
{
    public sealed class EquipmentItem : BaseItem, IEquipmentToggleTarget, ISerializable<EquipmentItemMemento>
    {
        private readonly ReactiveProperty<bool> _equippedState;
        private readonly List<ArtifactPassiveConditionBundle> _passiveConditionSlots;

        public int SlotLimit { get; }

        public IReadOnlyList<ArtifactPassiveConditionBundle> PassiveConditionSlots => _passiveConditionSlots;

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

        public override void LogWhyCannotActivateWhenUsed(IActor actor, IMap map)
        {
            if (IsCursed && _equippedState.CurrentValue)
            {
                LogCannotUnequipWhileCursed(actor, map);
            }
        }

        private void LogCannotUnequipWhileCursed(IActorOfEffect actor, IMap map)
        {
            GameLog.Add(actor.IsVisible, $"{GetName(map.Player, map.ItemPlaceholders)}は呪われていて外せない");
        }

        public bool TryToggleEquipped(IActorOfEffect actor, IMap map)
        {
            if (IsCursed && _equippedState.CurrentValue)
            {
                LogCannotUnequipWhileCursed(actor, map);
                return false;
            }

            _equippedState.Value = !_equippedState.CurrentValue;
            _onItemUpdated.OnNext(Unit.Default);
            return true;
        }

        public override void Repair(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders)
        {
        }

        public override async UniTask<ISkillResult> Use(IActor actor, Vector2Int position, Direction8 direction,
            IMap map)
        {
            if (ShouldRevealMimic(actor, position, map))
            {
                return SpawnEffectSkillResult.Failed;
            }

            SetCurseIdentified(true, map.Player, actor, map.ItemPlaceholders);

            if (!actor.CanReadItem && RequiresLiteracy)
            {
                GameLog.Add(actor.IsVisible, $"{GetName(map.Player, map.ItemPlaceholders)}は文字が読めない");
                return SpawnEffectSkillResult.Failed;
            }

            var skill = SkillOnUse.Expect("SkillOnUse is null");

            if (!skill.IsUsable())
            {
                GameLog.Add(actor.IsVisible, $"しかしうまくいかなかった");
                return SpawnEffectSkillResult.Failed;
            }

            var result = await skill.Use(actor, this, position, direction, map);
            if (result.Result != SkillResult.Cancelled)
            {
                if (State == ItemState.ShopItem)
                {
                    SetState(ItemState.UsedShopItem);
                }

                _onItemUpdated.OnNext(Unit.Default);
            }

            return result;
        }

        public override async UniTask<ISkillResult> UseWhenThrown(IActorOfEffect actor, Vector2Int position,
            Direction8 direction, IMap map)
        {
            if (ShouldRevealMimic(actor, position, map))
            {
                return SpawnEffectSkillResult.Failed;
            }

            SetCurseIdentified(true, map.Player, actor, map.ItemPlaceholders);

            if (!actor.CanReadItem && RequiresLiteracy)
            {
                GameLog.Add(actor.IsVisible, $"{GetName(map.Player, map.ItemPlaceholders)}は文字が読めない");
                return SpawnEffectSkillResult.Failed;
            }

            if (!SkillOnThrow.HasValue)
            {
                return SpawnEffectSkillResult.Failed;
            }

            var skill = SkillOnThrow.Expect("SkillOnThrow is null");

            if (!skill.IsUsable())
            {
                return SpawnEffectSkillResult.Failed;
            }

            var result = await SkillExtension.Match(
                skill.Skill,
                spawnEffectSkill => spawnEffectSkill.Use(actor, position, direction, map),
                itemTargetSkill => throw new Exception(
                    "The item is not configured to activate this type of skill when thrown."),
                inventoryTargetSkill => throw new Exception(
                    "The item is not configured to activate this type of skill when thrown."),
                equipToggleSkill => equipToggleSkill.Use(actor, this, position, direction, map)
            );
            if (result.Result != SkillResult.Cancelled)
            {
                if (State == ItemState.ShopItem)
                {
                    SetState(ItemState.UsedShopItem);
                }

                _onItemUpdated.OnNext(Unit.Default);
            }

            return result;
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

        public bool CanMergeFrom(EquipmentItem material) =>
            _passiveConditionSlots.Count < SlotLimit
            && material.PassiveConditionSlots.Count > 0;

        public EquipmentItem Merge(IItem mergedItem)
        {
            if (mergedItem is not EquipmentItem other)
            {
                throw new ArgumentException("Equipment item can only be merged with other equipment items");
            }

            var memento = Serialize();
            var newSlots = memento.PassiveConditionSlots.Select(b => b.Clone()).ToList();
            foreach (var bundle in other.PassiveConditionSlots)
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

        public override void Upgrade(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders, bool log = true) =>
            throw new Exception("Cannot upgrade equipment item");

        public override void Downgrade(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders, bool log = true) =>
            throw new Exception("Cannot downgrade equipment item");

        protected override string? BuildTemplatedActivatableSkillInfo() => null;

        protected override string FullInfoImpl()
        {
            var info = $"\nパッシブスキル ({_passiveConditionSlots.Count}/{SlotLimit})\n";

            foreach (var bundle in _passiveConditionSlots)
                info += $"{bundle.DisplayName}\n";
            return info;
        }
    }
}
