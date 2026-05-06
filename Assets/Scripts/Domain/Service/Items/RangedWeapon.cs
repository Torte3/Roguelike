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
    public class RangedWeapon : BaseItem, ISerializable<RangedWeaponMemento>
    {
        public override string RevealedName => _prefix.MapOr("", prefix => prefix.Name) + BaseName;
        public override ItemCategory Category => ItemCategory.Weapons;
        private bool _hasSameEffect;
        protected override bool HasSameEffect => _hasSameEffect;
        protected override bool HasSameSkill => false;
        public override bool UseOnDeath => false;
        public override bool RequiresLiteracy => false;
        public override bool IdentifyIfGot => true;
        public override bool IdentifyIfUsed => true;
        public override bool AutoDestroyWhenDisabled => false;
        private readonly Option<WeaponPrefix> _prefix;
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

        public RangedWeapon(RangedWeaponMemento data) : base(data.BaseItem)
        {
            _prefix = data.Prefix;
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
                prefix: _prefix,
                defaultPower: _defaultPower,
                projectileIcon: _projectileIcon,
                features: _features,
                featureLimit: FeatureLimit,
                skillOnUse: _skillOnUse.Serialize()
            ));
            return JsonUtility.FromJson<RangedWeaponMemento>(json);
        }

        public override bool CanUpgrade() => UpgradeCount < UpgradeLimit;
        public override bool CanDowngrade() => UpgradeCount > 0;
        public override void Upgrade(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders)
        {
            GameLog.Add(itemHolder.IsVisible, $"{GetName(player, itemPlaceholders)}は強化された");
            UpgradeCount++;
            var skillOnUse = BuildSkills(
                _defaultPower,
                UpgradeCount,
                _projectileIcon,
                _features,
                _prefix.Value
            );
            _skillOnUse = new SkillWithCost(skillOnUse);
            _onItemUpdated.OnNext(Unit.Default);
        }

        public override void Downgrade(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders)
        {
            GameLog.Add(itemHolder.IsVisible, $"{GetName(player, itemPlaceholders)}は強化が解除された");
            UpgradeCount--;
            var skillOnUse = BuildSkills(
                _defaultPower,
                UpgradeCount,
                _projectileIcon,
                _features,
                _prefix.Value
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
            var elementPowers = new List<ElementPower>();
            var powerMagnification = 1f;
            if (prefix != null)
            {
                powerMagnification *= prefix.PowerMagnification;
            }
            if (features.Contains(ItemFeature.ChargeAttack))
            {
                powerMagnification *= 1.8f;
            }
            power = Mathf.RoundToInt(power * powerMagnification);
            power += upgradeCount;

            var elementFeatureMapping = new Dictionary<ItemFeature, Element>
            {
                { ItemFeature.Fire, Element.Fire },
                { ItemFeature.Ice, Element.Ice },
                { ItemFeature.Thunder, Element.Thunder },
                { ItemFeature.Light, Element.Light },
                { ItemFeature.Dark, Element.Dark }
            };

            var elementFeature = elementFeatureMapping.Keys.FirstOrDefault(feature => features.Contains(feature));

            List<ElementPower> CreateElementPowers(int powerValue)
            {
                var elementPowers = new List<ElementPower>();
                if (elementFeature != default)
                {
                    var element = elementFeatureMapping[elementFeature];
                    var elementPower = Mathf.CeilToInt(powerValue / 2f);
                    elementPowers.Add(new ElementPower(element, elementPower));
                    elementPowers.Add(new ElementPower(Element.Physical, powerValue - elementPower));
                }
                else
                {
                    elementPowers.Add(new ElementPower(Element.Physical, powerValue));
                }
                return elementPowers;
            }

            elementPowers = CreateElementPowers(power);

            var criticalRate = features.Count(f => f == ItemFeature.Critical) * 0.25f;
            if (features.Contains(ItemFeature.Absorbing))
            {
                var absorbRate = features.Count(f => f == ItemFeature.Absorbing) * 0.25f;
                effectsOnUse.Add(new AbsorbsEffect(
                    elementPowers,
                    absorbRate,
                    criticalRate
                ));
            }
            else
            {
                effectsOnUse.Add(new AttackEffect(
                    elementPowers,
                    criticalRate
                ));
            }
            if (features.Contains(ItemFeature.Knockback))
            {
                effectsOnUse.Add(new BlowAwayEffect(1));
            }
            if (features.Contains(ItemFeature.Dig))
            {
                effectsOnUse.Add(new DigEffect());
            }
            if (features.Contains(ItemFeature.BreakTrap))
            {
                effectsOnUse.Add(new BreakEffect(false, false, false, true, false, false));
            }
            var abnormalConditionMultiplier = features.Count(f => f == ItemFeature.EnhanceAbnormalCondition) + 1;

            // 状態異常フィーチャーから対応するデータを取得
            var conditionFeatureMapping = new Dictionary<ItemFeature, (string templateName, float baseProbability)>
            {
                { ItemFeature.Paralysis, ("麻痺", 0.05f) },
                { ItemFeature.Blind, ("盲目", 0.1f) },
                { ItemFeature.Confusion, ("混乱", 0.1f) },
                { ItemFeature.Sleep, ("睡眠", 0.05f) },
                { ItemFeature.Poison, ("毒", 0.2f) },
                { ItemFeature.Slowness, ("鈍足", 0.1f) },
                { ItemFeature.Restraint, ("拘束", 0.1f) }
            };

            foreach (var (feature, (templateName, baseProbability)) in conditionFeatureMapping)
            {
                if (features.Contains(feature))
                {
                    var probability = baseProbability * abnormalConditionMultiplier;
                    var conditionData = new AdditionalConditionData(
                        ObjectLoader.Load<ConditionTemplate>(templateName), probability);
                    effectsOnUse.Add(new AddConditionEffect(conditionData));
                }
            }

            int repeat;
            if (features.Contains(ItemFeature.TripleAttack))
                repeat = 3;
            else if (features.Contains(ItemFeature.DoubleAttack))
                repeat = 2;
            else
                repeat = 1;

            var skillOnUseProbabilityOfSuccess = features.Contains(ItemFeature.GuaranteedHit) ? 1f : features.Contains(ItemFeature.Critical) ? 0.75f : CommonSenseParameters.SkillOnUseProbabilityOfSuccess;

            var chargeTurn = features.Contains(ItemFeature.ChargeAttack) ? 1 : 0;

            return SkillWithCost.Build(
                new SkillData(
                    position,
                    area,
                    effectsOnUse,
                    repeat,
                    skillOnUseProbabilityOfSuccess,
                    0,
                    0,
                    0,
                    chargeTurn,
                    0,
                    ""
                )
            );
        }

        public static RangedWeaponMemento Build(RangedWeaponData data, int upgradeCount = 0, WeaponPrefix? prefix = null, bool isCursed = false, ItemState state = ItemState.None, EnemyData? mimic = null)
        {
            var skillOnUse = BuildSkills(data.Power, 0, data.ProjectileIcon, data.Features, prefix);
            var multiplyPrice = data.Features.Contains(ItemFeature.Artistic) ? 2f : 1f;
            var usageLossChance = 1 - data.Features.Count(f => f == ItemFeature.EnhanceDurability) * 0.2f;
            var featureLimit = data.FeatureLimit + prefix?.FeatureLimitAdditional ?? 0;
            var maxUsages = Mathf.RoundToInt(data.UsageLimit * (prefix?.UsageLimitMagnification ?? 1f));

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
                    isCursed: isCursed,
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
            var multiplyPrice = features.Contains(ItemFeature.Artistic) ? 2f : 1f;
            var usageLossChance = 1 - features.Count(f => f == ItemFeature.EnhanceDurability) * 0.2f;
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
            rangedWeapon => Merge(rangedWeapon.Features, rangedWeapon.UpgradeCount)
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