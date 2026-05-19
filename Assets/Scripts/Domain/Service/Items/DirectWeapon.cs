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
using Utilities.Serialize.Option;

namespace Domain.Service.Items
{
    public class DirectWeapon : WeaponConsumableItem, ISerializable<DirectWeaponMemento>
    {
        private readonly int _defaultPower;
        private readonly List<ItemFeature> _features;
        public IReadOnlyList<ItemFeature> Features => _features;
        public readonly int FeatureLimit;
        private ISkillWithCost _skillOnUse;
        private ISkillWithCost _skillOnThrow;
        public override Option<ISkillWithCost> SkillOnUse => _skillOnUse.ToOption();
        public override Option<ISkillWithCost> SkillOnThrow => _skillOnThrow.ToOption();
        public DirectWeapon(DirectWeaponData data) : this(Build(data))
        {
        }

        public DirectWeapon(DirectWeaponMemento data) : base(data.BaseItem, data.Prefix)
        {
            _hasSameEffect = data.HasSameEffect;
            _defaultPower = data.DefaultPower;
            _features = data.Features;
            FeatureLimit = data.FeatureLimit;
            _skillOnUse = new SkillWithCost(data.SkillOnUse);
            _skillOnThrow = new SkillWithCost(data.SkillOnThrow);
        }

        public DirectWeaponMemento Serialize()
        {
            var json = JsonUtility.ToJson(new DirectWeaponMemento
            (
                baseItem: SerializeBase(),
                prefix: WeaponPrefix,
                defaultPower: _defaultPower,
                features: _features,
                featureLimit: FeatureLimit,
                skillOnUse: _skillOnUse.Serialize(),
                skillOnThrow: _skillOnThrow.Serialize(),
                hasSameEffect: _hasSameEffect
            ));
            return JsonUtility.FromJson<DirectWeaponMemento>(json);
        }

        public override void Upgrade(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders, bool log = true)
        {
            if (log)
                GameLog.Add(itemHolder.IsVisible, $"{GetName(player, itemPlaceholders)}は強化された");
            UpgradeCount++;
            var (skillOnUse, skillOnThrow, hasSameEffect) = BuildSkills(_defaultPower, UpgradeCount, _features, WeaponPrefix.Value);
            _skillOnUse = new SkillWithCost(skillOnUse);
            _skillOnThrow = new SkillWithCost(skillOnThrow);
            _hasSameEffect = hasSameEffect;
            _onItemUpdated.OnNext(Unit.Default);
        }

        public override void Downgrade(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders, bool log = true)
        {
            if (log)
                GameLog.Add(itemHolder.IsVisible, $"{GetName(player, itemPlaceholders)}は強化が解除された");
            UpgradeCount--;
            var (skillOnUse, skillOnThrow, hasSameEffect) = BuildSkills(_defaultPower, UpgradeCount, _features, WeaponPrefix.Value);
            _skillOnUse = new SkillWithCost(skillOnUse);
            _skillOnThrow = new SkillWithCost(skillOnThrow);
            _hasSameEffect = hasSameEffect;
            _onItemUpdated.OnNext(Unit.Default);
        }
        public static (SkillWithCostMemento skillOnUse, SkillWithCostMemento skillOnThrow, bool hasSameEffect) BuildSkills(int power, int upgradeCount, List<ItemFeature> features, WeaponPrefix? prefix = null)
        {
            var range = features.Contains(ItemFeature.TwoRangeAttack) ? 2 : 1;
            var area = (IArea)new LineArea(range, false, false);
            if (features.Contains(ItemFeature.FanAttack))
            {
                area = new FanArea(range, false, false);
            }
            else if (features.Contains(ItemFeature.SpinAttack))
            {
                area = new CircleArea(range, false, false);
            }

            var effectsOnUse = new List<IEffect>();
            var effectsOnThrow = new List<IEffect>();

            var usePower = WeaponFeatureSkillBuilder.CalculateAttackPower(power, upgradeCount, features, prefix, applyChargeAttackMagnification: true);
            var throwPower = WeaponFeatureSkillBuilder.CalculateAttackPower(
                power,
                upgradeCount,
                features,
                prefix,
                applyChargeAttackMagnification: false);
            if (features.Contains(ItemFeature.EnhanceThrow))
                throwPower = Mathf.RoundToInt((throwPower - upgradeCount) * 1.5f) + upgradeCount;

            var elementPowersOnUse = WeaponFeatureSkillBuilder.CreateElementPowers(usePower, features);
            var elementPowersOnThrow = WeaponFeatureSkillBuilder.CreateElementPowers(throwPower, features);

            var criticalRate = WeaponFeatureSkillBuilder.GetCriticalRate(features);
            var hasSameEffect = usePower == throwPower;
            WeaponFeatureSkillBuilder.AddCombatEffects(effectsOnUse, features, elementPowersOnUse, criticalRate);
            WeaponFeatureSkillBuilder.AddCombatEffects(effectsOnThrow, features, elementPowersOnThrow, criticalRate, isWeaponAttack: false);

            var repeat = WeaponFeatureSkillBuilder.GetAttackRepeatCount(features);
            var skillOnUseProbabilityOfSuccess = WeaponFeatureSkillBuilder.GetSkillOnUseProbabilityOfSuccess(features);
            var skillOnThrowProbabilityOfSuccess = WeaponFeatureSkillBuilder.GetSkillOnThrowProbabilityOfSuccess(features);

            var rushDistance = features.Contains(ItemFeature.Lunge) ? 1 : 0;
            var backStepDistance = features.Contains(ItemFeature.BackStep) ? 1 : 0;
            var chargeTurn = WeaponFeatureSkillBuilder.GetChargeTurn(features);

            var skillOnUse = SkillWithCost.Build(
                new SkillDataOnUse(
                    new AtFeet(),
                    area,
                    effectsOnUse,
                    repeat,
                    skillOnUseProbabilityOfSuccess,
                    0,
                    rushDistance,
                    backStepDistance,
                    chargeTurn,
                    0
                )
            );
            var skillOnThrow = SkillWithCost.Build(
                new SkillDataOnThrow(
                    new SelfArea(),
                    effectsOnThrow,
                    skillOnThrowProbabilityOfSuccess
                )
            );
            return (skillOnUse, skillOnThrow, hasSameEffect);
        }

        public static DirectWeaponMemento Build(DirectWeaponData data, int upgradeCount = 0, WeaponPrefix? prefix = null, bool isCursed = false, ItemState state = ItemState.None, EnemyData? mimic = null)
        {
            var (skillOnUse, skillOnThrow, hasSameEffect) = BuildSkills(data.Power, 0, data.Features, prefix);
            var multiplyPrice = WeaponFeatureSkillBuilder.GetMultiplyPrice(data.Features);
            var usageLossChance = WeaponFeatureSkillBuilder.GetUsageLossChance(data.Features);
            var featureLimit = data.FeatureLimit + prefix?.FeatureLimitAdditional ?? 0;
            var maxUsages = Mathf.RoundToInt(data.UsageLimit * (prefix?.UsageLimitMagnification ?? 1f));
            var isCursedByPrefix = prefix != null && prefix.IsCursed;

            var json = JsonUtility.ToJson(new DirectWeaponMemento
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
                features: data.Features,
                featureLimit: data.FeatureLimit,
                skillOnUse: skillOnUse,
                skillOnThrow: skillOnThrow,
                hasSameEffect: hasSameEffect
            ));
            var item = JsonUtility.FromJson<DirectWeaponMemento>(json); //MEMO: To break the sharing references
            return item;
        }

        private DirectWeapon Merge(IEnumerable<ItemFeature> featuresToMergeWeapon, int additionalUpgrade)
        {
            var memento = Serialize();
            var features = memento.Features.Merge(featuresToMergeWeapon, memento.FeatureLimit, FeatureApplicabilityTag.DirectWeapons).ToList();

            var (skillOnUse, skillOnThrow, hasSameEffect) = BuildSkills(memento.DefaultPower, memento.BaseItem.UpgradeCount + additionalUpgrade, features, memento.Prefix.Value);
            var multiplyPrice = WeaponFeatureSkillBuilder.GetMultiplyPrice(features);
            var usageLossChance = WeaponFeatureSkillBuilder.GetUsageLossChance(features);
            var item = new DirectWeapon(memento.CopyWith(
                baseItem: memento.BaseItem.CopyWith(
                    multiplyPrice: multiplyPrice,
                    upgradeCount: memento.BaseItem.UpgradeCount + additionalUpgrade,
                    usageLossChance: usageLossChance
                ),
                features: features,
                skillOnUse: skillOnUse,
                skillOnThrow: skillOnThrow,
                hasSameEffect: hasSameEffect
            ));
            return item;
        }

        public DirectWeapon Merge(IItem mergedItem) => mergedItem.Match(
            item => Merge(item.FeaturesToMergeWeapon, item.UpgradeCount),
            directWeapon => Merge(directWeapon.Features, directWeapon.UpgradeCount),
            rangedWeapon => Merge(rangedWeapon.Features, rangedWeapon.UpgradeCount),
            _ => throw new ArgumentException(
                "Invalid merge target: only another weapon or an item with mergeable weapon features is allowed.")
        );

        protected override string? BuildTemplatedActivatableSkillInfo() =>
            ItemDescriptionTemplate.FormatDirectWeapon(
                (SkillWithCost)SkillOnUse.Expect("SkillOnUse is null"),
                (SkillWithCost)SkillOnThrow.Expect("SkillOnThrow is null"),
                _hasSameEffect);

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