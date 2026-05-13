#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Memento;
using Domain.Service.Characters;
using Domain.Service.Effect;
using UnityEngine;
using Utilities.Serialize.Option;

namespace Domain.Service.Items
{
    public class Item : ConsumableItem, ISerializable<ItemMemento>
    {
        private readonly ItemCategory _category;
        private readonly Option<ISkillWithCost> _skillOnUse;
        private readonly Option<ISkillWithCost> _skillOnThrow;
        private readonly bool _useOnDeath;

        public override string RevealedName => BaseName;
        public override ItemCategory Category => _category;
        public override Option<ISkillWithCost> SkillOnUse => _skillOnUse;
        public override Option<ISkillWithCost> SkillOnThrow => _skillOnThrow;
        protected override bool HasSameEffect => _hasSameEffect;
        protected override bool HasSameSkill => _hasSameSkill;
        public override bool UseOnDeath => _useOnDeath;
        public bool CanMergeUses => Category == ItemCategory.Books || Category == ItemCategory.Wands;
        public override bool RequiresLiteracy => Category == ItemCategory.Books || Category == ItemCategory.Scrolls;
        public override bool IdentifyIfGot => Category == ItemCategory.Weapons || Category == ItemCategory.Others;
        public override bool IdentifyIfUsed => Category != ItemCategory.Wands;
        public override bool AutoDestroyWhenDisabled => Category == ItemCategory.Potions || Category == ItemCategory.Scrolls || Category == ItemCategory.Others;
        public readonly IReadOnlyList<ItemFeature> FeaturesToMergeWeapon;

        public Item(ItemData data) : this(Build(data))
        {
        }

        public Item(ItemMemento data) : base(
            data.BaseItem)
        {
            _category = data.Category;
            _hasSameEffect = data.HasSameEffect;
            _hasSameSkill = data.HasSameSkill;
            _skillOnUse = data.SkillOnUse.Map(skill => (ISkillWithCost)new SkillWithCost(skill));
            if (data.HasSameEffect)
            {
                var skillOnUse = _skillOnUse.Expect("SkillOnUse is null").Serialize();
                if (skillOnUse.Skill is not SpawnEffectSkillMemento spawnEffectSkillOnUse)
                {
                    throw new Exception("SkillOnUse is not SpawnEffectSkill");
                }
                var skillOnThrow = data.SkillOnThrow.Expect("SkillOnThrow is null");
                if (skillOnThrow.Skill is not SpawnEffectSkillMemento spawnEffectSkillOnThrow)
                {
                    throw new Exception("SkillOnThrow is not SpawnEffectSkill");
                }
                _skillOnThrow = Option.Some<ISkillWithCost>(
                    new SkillWithCost(
                        SkillWithCost.Build(
                            spawnEffectSkillOnThrow.CopyWith(
                                effect: spawnEffectSkillOnUse.Effects
                            ),
                            skillOnThrow.Cost,
                            skillOnThrow.ChargeTurn,
                            skillOnThrow.CoolTime
                        )
                    )
                );
            }
            else if (data.HasSameSkill)
            {
                var skillOnUse = _skillOnUse.Expect("SkillOnUse is null").Serialize();
                if (skillOnUse.Skill is not SpawnEffectSkillMemento spawnEffectSkillOnUse)
                {
                    throw new Exception("SkillOnUse is not SpawnEffectSkill");
                }
                var skillOnThrow = data.SkillOnThrow.Expect("SkillOnThrow is null");
                if (skillOnThrow.Skill is not SpawnEffectSkillMemento spawnEffectSkillOnThrow)
                {
                    throw new Exception("SkillOnThrow is not SpawnEffectSkill");
                }
                _skillOnThrow = Option.Some<ISkillWithCost>(
                    new SkillWithCost(
                        SkillWithCost.Build(
                            spawnEffectSkillOnThrow.CopyWith(
                                position: spawnEffectSkillOnUse.Position,
                                area: spawnEffectSkillOnUse.Area,
                                effect: spawnEffectSkillOnUse.Effects
                            ),
                            skillOnThrow.Cost,
                            skillOnThrow.ChargeTurn,
                            skillOnThrow.CoolTime
                        )
                    )
                );
            }
            else
            {
                _skillOnThrow = data.SkillOnThrow.Map(skill => (ISkillWithCost)new SkillWithCost(skill));
            }

            _useOnDeath = data.UseOnDeath;
            FeaturesToMergeWeapon = data.FeaturesToMergeWeapon;
        }

        private readonly bool _hasSameEffect;
        private readonly bool _hasSameSkill;

        public ItemMemento Serialize()
        {
            var json = JsonUtility.ToJson(new ItemMemento
            (
                baseItem: SerializeBase(),
                category: _category,
                skillOnUse: _skillOnUse.Map(skill => skill.Serialize()),
                skillOnThrow: _skillOnThrow.Map(skill => skill.Serialize()),
                hasSameEffect: _hasSameEffect,
                hasSameSkill: _hasSameSkill,
                useOnDeath: _useOnDeath,
                featuresToMergeWeapon: FeaturesToMergeWeapon.ToList()
            ));
            return JsonUtility.FromJson<ItemMemento>(json);
        }

        public static ItemMemento Build(ItemData data, int upgradeCount = 0, bool isCursed = false, ItemState state = ItemState.None, EnemyData? mimic = null)
        {
            var skillOnUse = data.EffectType switch
            {
                ItemEffectType.SpawnEffect => data.SpawnEffectsOnUse
                    ? SkillWithCost.Build(data.SkillOnUse)
                    : null,
                ItemEffectType.ItemTarget => SkillWithCost.Build(ItemTargetSkill.Build(data.ItemEffect), 0, 0, 0),
                ItemEffectType.InventoryTarget => SkillWithCost.Build(InventoryTargetSkill.Build(data.InventoryEffect), 0, 0, 0),
                ItemEffectType.None => null,
                _ => throw new Exception("Invalid item effect type")
            };
            var skillOnThrow = data.SpawnEffectsOnThrow
                ? SkillWithCost.Build(data.SkillOnThrow)
                : null;

            var json = JsonUtility.ToJson(new ItemMemento
            (
                baseItem: BuildBase(
                    baseName: data.name,
                    icon: data.Icon,
                    isShiny: data.IsShiny,
                    rarity: data.Rarity,
                    customBasePrice: data.UseCustomBasePrice ? data.CustomBasePrice : null,
                    additionalPrice: data.AdditionalPrice,
                    multiplyPrice: data.MultiplyPrice,
                    state: state,
                    upgradeCount: upgradeCount,
                    maxUsages: data.UsageLimit,
                    usageLossChance: 1,
                    isCursed: isCursed,
                    upgradeLimit: data.UpgradeLimit,
                    conditions: data.PassiveConditions,
                    mimic: mimic.ToOption(),
                    isEquipped: Option.None<bool>()
                ),
                category: data.Category,
                skillOnUse: skillOnUse.ToOption(),
                skillOnThrow: skillOnThrow.ToOption(),
                hasSameEffect: data.IsSameEffect,
                hasSameSkill: data.IsSameSkill,
                useOnDeath: data.UseOnDeath,
                featuresToMergeWeapon: data.FeaturesToMergeWeapon
            ));
            return JsonUtility.FromJson<ItemMemento>(json); //MEMO: To break the sharing of references
        }

        public override bool CanUpgrade() => false;
        public override bool CanDowngrade() => false;
        public override void Upgrade(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders, bool log = true) =>
            throw new Exception("Cannot upgrade item");
        public override void Downgrade(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders, bool log = true) =>
            throw new Exception("Cannot downgrade item");

        protected override string? BuildTemplatedActivatableSkillInfo()
        {
            if (Category != ItemCategory.Potions)
                return null;
            var info = ItemDescriptionTemplate.FormatPotion(
                _skillOnUse.UnwrapOrNull() is { } u ? (SkillWithCost)u : null,
                _skillOnThrow.UnwrapOrNull() is { } t ? (SkillWithCost)t : null,
                _hasSameEffect,
                _hasSameSkill);
            return string.IsNullOrEmpty(info) ? null : info;
        }

        protected override string FullInfoImpl() => "";

        public Item Merge(Item mergedItem)
        {
            if (BaseName != mergedItem.BaseName)
            {
                throw new Exception("Cannot merge different items");
            }
            else if (!CanMergeUses)
            {
                throw new Exception("Cannot merge uses");
            }
            var memento = Serialize();
            var mergedMemento = mergedItem.Serialize();
            var item = new Item(memento.CopyWith(
                baseItem: memento.BaseItem.CopyWith(
                    maxUsages: memento.BaseItem.MaxUsages + mergedMemento.BaseItem.MaxUsages,
                    remainingUsages: memento.BaseItem.RemainingUsages + mergedMemento.BaseItem.RemainingUsages
                )
            ));
            return item;
        }
    }
}