#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Effect.Area;
using Domain.Model.Effect.Position;
using Domain.Model.Entity;
using Domain.Model.Evaluation;
using Domain.Model.Item;
using Domain.Model.Memento;
using Domain.Service.Characters;
using Domain.Service.Effect;
using Domain.Service.Logs;
using R3;
using UnityEngine;
using Utilities;
using Utilities.Serialize;
using Utilities.Serialize.Option;

namespace Domain.Service.Items
{
    public class RangedWeapon : WeaponConsumableItem, ISerializable<RangedWeaponMemento>
    {
        private readonly int _defaultPower;
        private readonly IconSerializable _projectileIcon;
        private readonly List<ItemFeature> _features;
        public IReadOnlyList<ItemFeature> Features => _features;
        public readonly int FeatureLimit;
        private ISkillWithCost _skillOnUse;
        public override Option<ISkillWithCost> SkillOnUse => _skillOnUse.ToOption();
        public override Option<ISkillWithCost> SkillOnThrow => Option.None<ISkillWithCost>();
        public RangedWeapon(RangedWeaponData data) : this(Build(data))
        {
        }

        public RangedWeapon(RangedWeaponMemento data) : base(data.BaseItem, data.Prefix)
        {
            _defaultPower = data.DefaultPower;
            _projectileIcon = data.ProjectileIcon;
            _features = data.Features;
            FeatureLimit = data.FeatureLimit;
            _skillOnUse = new SkillWithCost(data.SkillOnUse);
        }

        public RangedWeaponMemento Serialize()
        {
            var json = JsonUtility.ToJson(new RangedWeaponMemento
            (
                baseItem: SerializeBase(),
                prefix: WeaponPrefix,
                defaultPower: _defaultPower,
                projectileIcon: _projectileIcon,
                features: _features,
                featureLimit: FeatureLimit,
                skillOnUse: _skillOnUse.Serialize()
            ));
            return JsonUtility.FromJson<RangedWeaponMemento>(json);
        }

        public override void Upgrade(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders, bool log = true)
        {
            if (log)
                GameLog.Add(itemHolder.IsVisible, $"{GetName(player, itemPlaceholders)}は強化された");
            UpgradeCount++;
            var skillOnUse = BuildSkills(
                _defaultPower,
                UpgradeCount,
                _projectileIcon,
                _features,
                WeaponPrefix.Value
            );
            _skillOnUse = new SkillWithCost(skillOnUse);
            _onItemUpdated.OnNext(Unit.Default);
        }

        public override void Downgrade(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders, bool log = true)
        {
            if (log)
                GameLog.Add(itemHolder.IsVisible, $"{GetName(player, itemPlaceholders)}は強化が解除された");
            UpgradeCount--;
            var skillOnUse = BuildSkills(
                _defaultPower,
                UpgradeCount,
                _projectileIcon,
                _features,
                WeaponPrefix.Value
            );
            _skillOnUse = new SkillWithCost(skillOnUse);
            _onItemUpdated.OnNext(Unit.Default);
        }
        public static SkillWithCostMemento BuildSkills(int power, int upgradeCount, IconSerializable projectileIcon, List<ItemFeature> features, WeaponPrefix? prefix = null)
        {
            var position = (IEffectPosition)new ProjectileImpact(projectileIcon, new List<EntityLayer> { EntityLayer.Middle }, features.Contains(ItemFeature.Piercing));
            if (features.Contains(ItemFeature.ArcingShot))
            {
                position = new NearByCharacter(1, false, true, false, false);
            }

            var area = (IArea)new SelfArea();
            if (features.Contains(ItemFeature.Explosive))
            {
                area = new CircleArea(1, true, false);
            }

            var effectsOnUse = new List<IEffect>();
            var attackPower = WeaponFeatureSkillBuilder.CalculateAttackPower(
                power, upgradeCount, features, prefix, applyChargeAttackMagnification: true);
            var elementPowers = WeaponFeatureSkillBuilder.CreateElementPowers(attackPower, features);
            var criticalRate = WeaponFeatureSkillBuilder.GetCriticalRate(features);
            WeaponFeatureSkillBuilder.AddCombatEffects(effectsOnUse, features, elementPowers, criticalRate);

            var repeat = WeaponFeatureSkillBuilder.GetAttackRepeatCount(features);
            var skillOnUseProbabilityOfSuccess = WeaponFeatureSkillBuilder.GetSkillOnUseProbabilityOfSuccess(features);
            var chargeTurn = WeaponFeatureSkillBuilder.GetChargeTurn(features);
            var backStepDistance = features.Contains(ItemFeature.BackStep) ? 1 : 0;
            var skillHpCost = WeaponFeatureSkillBuilder.GetSkillHpCost(features);

            return SkillWithCost.Build(
                new SkillData(
                    position,
                    area,
                    effectsOnUse,
                    repeat,
                    skillOnUseProbabilityOfSuccess,
                    skillHpCost,
                    0,
                    backStepDistance,
                    chargeTurn,
                    0,
                    ""
                )
            );
        }

        public static RangedWeaponMemento Build(RangedWeaponData data, int upgradeCount = 0, WeaponPrefix? prefix = null, bool isCursed = false, ItemState state = ItemState.None, EnemyData? mimic = null)
        {
            var skillOnUse = BuildSkills(data.Power, 0, data.ProjectileIcon, data.Features, prefix);
            var multiplyPrice = WeaponFeatureSkillBuilder.GetMultiplyPrice(data.Features);
            var usageLossChance = WeaponFeatureSkillBuilder.GetUsageLossChance(data.Features);
            var featureLimit = data.FeatureLimit + prefix?.FeatureLimitAdditional ?? 0;
            var maxUsages = Mathf.RoundToInt(data.UsageLimit * (prefix?.UsageLimitMagnification ?? 1f));
            var isCursedByPrefix = prefix != null && prefix.IsCursed;

            var json = JsonUtility.ToJson(new RangedWeaponMemento
            (
                baseItem: BuildBase(
                    baseName: data.name,
                    icon: data.Icon,
                    isShiny: data.IsShiny,
                    rarity: data.Rarity,
                    customBasePrice: data.UseCustomBasePrice ? data.CustomBasePrice : null,
                    additionalPrice: 0,
                    multiplyPrice: multiplyPrice,
                    state: state,
                    upgradeCount: upgradeCount,
                    maxUsages: maxUsages,
                    usageLossChance: usageLossChance,
                    isCursed: isCursed || isCursedByPrefix,
                    upgradeLimit: data.UpgradeLimit + prefix.ToOption().MapOr(0, prefix => prefix.AdditionalUpgradeLimit),
                    conditions: data.PassiveConditions,
                    mimic: mimic.ToOption()
                ),
                prefix: prefix.ToOption(),
                defaultPower: data.Power,
                projectileIcon: data.ProjectileIcon,
                features: data.Features,
                featureLimit: data.FeatureLimit,
                skillOnUse: skillOnUse
            ));
            var item = JsonUtility.FromJson<RangedWeaponMemento>(json); //MEMO: To break the sharing references
            return item;
        }

        private RangedWeapon Merge(IEnumerable<ItemFeature> featuresToMergeWeapon, int additionalUpgrade)
        {
            var memento = Serialize();
            var features = memento.Features.Merge(featuresToMergeWeapon, memento.FeatureLimit, FeatureApplicabilityTag.RangedWeapons).ToList();

            var skillOnUse = BuildSkills(
                memento.DefaultPower,
                memento.BaseItem.UpgradeCount + additionalUpgrade,
                memento.ProjectileIcon,
                features,
                memento.Prefix.Value
            );
            var multiplyPrice = WeaponFeatureSkillBuilder.GetMultiplyPrice(features);
            var usageLossChance = WeaponFeatureSkillBuilder.GetUsageLossChance(features);
            var item = new RangedWeapon(memento.CopyWith(
                baseItem: memento.BaseItem.CopyWith(
                    multiplyPrice: multiplyPrice,
                    upgradeCount: memento.BaseItem.UpgradeCount + additionalUpgrade,
                    usageLossChance: usageLossChance
                ),
                features: features,
                skillOnUse: skillOnUse
            ));
            return item;
        }

        public RangedWeapon Merge(IItem mergedItem) => mergedItem.Match(
            item => Merge(item.FeaturesToMergeWeapon, item.UpgradeCount),
            directWeapon => Merge(directWeapon.Features, directWeapon.UpgradeCount),
            rangedWeapon => Merge(rangedWeapon.Features, rangedWeapon.UpgradeCount),
            _ => throw new ArgumentException(
                "Invalid merge target: only another weapon or an item with mergeable weapon features is allowed.")
        );

        protected override string? BuildTemplatedActivatableSkillInfo() =>
            ItemDescriptionTemplate.FormatRangedWeapon((SkillWithCost)_skillOnUse);

        protected override string FullInfoImpl()
        {
            var info = "";

            info += $"能力 ({_features.Count}/{FeatureLimit})\n";

            foreach (var feature in _features)
            {
                info += $"{feature.GetName()}\n";
            }

            info += "\n";

            return info;
        }
    }
}