#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Condition;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Memento;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Service.Items
{
    public class Artifact : BaseItem, ISerializable<ArtifactMemento>
    {
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
        public override Option<ISkillWithCost> SkillOnUse => Option.None<ISkillWithCost>();
        public override Option<ISkillWithCost> SkillOnThrow => Option.None<ISkillWithCost>();

        public Artifact(ArtifactData data) : this(Build(data))
        {
        }

        public Artifact(ArtifactMemento data) : base(data.BaseItem)
        {
            _passiveConditionSlots = data.PassiveConditionSlots;
            SlotLimit = data.SlotLimit;
        }

        public ArtifactMemento Serialize()
        {
            var json = JsonUtility.ToJson(new ArtifactMemento(
                SerializeBase(),
                _passiveConditionSlots,
                SlotLimit));
            return JsonUtility.FromJson<ArtifactMemento>(json);
        }

        public static ArtifactMemento Build(
            ArtifactData data,
            bool isCursed = false,
            ItemState state = ItemState.None,
            EnemyData? mimic = null)
        {
            var hasBuiltIn = data.HasBuiltInPassive;
            var slots = new List<ArtifactPassiveConditionBundle>();
            if (hasBuiltIn)
            {
                slots.Add(data.BuiltInPassiveConditionBundle.Clone());
            }

            var slotLimit = (hasBuiltIn ? 1 : 0) + data.SynthesisSlotLimit;

            var conditions = FlattenConditionList(slots);
            var json = JsonUtility.ToJson(new ArtifactMemento(
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
                slotLimit));
            return JsonUtility.FromJson<ArtifactMemento>(json);
        }

        public bool CanMergeFrom(Artifact material) =>
            _passiveConditionSlots.Count < SlotLimit
            && material.PassiveConditionSlots.Count > 0;

        public Artifact Merge(IItem mergedItem)
        {
            if (mergedItem is not Artifact other)
            {
                throw new ArgumentException("アーティファクトは別のアーティファクトとだけ合成できます");
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
            return new Artifact(memento.CopyWith(
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
            throw new Exception("Cannot upgrade artifact");

        public override void Downgrade(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders, bool log = true) =>
            throw new Exception("Cannot downgrade artifact");

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
