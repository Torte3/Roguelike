#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character.Data;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Effect.Elements;
using Domain.Model.Evaluation;
using Domain.Model.Item;
using Domain.Model.Item.Data;
using Domain.Service.Effect;
using UnityEngine;
using Utilities;

namespace Domain.Service.Items
{
    /// <summary>
    /// 近接・射撃武器で共通する ItemFeature の戦闘効果組み立て。
    /// </summary>
    internal static class WeaponFeatureSkillBuilder
    {
        private static readonly Dictionary<ItemFeature, Element> ElementFeatureMapping = new()
        {
            { ItemFeature.Fire, Element.Fire },
            { ItemFeature.Ice, Element.Ice },
            { ItemFeature.Thunder, Element.Thunder },
            { ItemFeature.Light, Element.Light },
            { ItemFeature.Dark, Element.Dark }
        };

        private static readonly Dictionary<ItemFeature, (string templateName, float baseProbability)> ConditionFeatureMapping = new()
        {
            { ItemFeature.Paralysis, ("麻痺", 0.05f) },
            { ItemFeature.Blind, ("盲目", 0.1f) },
            { ItemFeature.Confusion, ("混乱", 0.1f) },
            { ItemFeature.Sleep, ("睡眠", 0.05f) },
            { ItemFeature.Poison, ("毒", 0.2f) },
            { ItemFeature.Slowness, ("鈍足", 0.1f) },
            { ItemFeature.Restraint, ("拘束", 0.1f) }
        };

        public static int CalculateAttackPower(
            int power,
            int upgradeCount,
            List<ItemFeature> features,
            WeaponPrefix? prefix,
            bool applyChargeAttackMagnification)
        {
            var powerMagnification = prefix?.PowerMagnification ?? 1f;
            if (applyChargeAttackMagnification && features.Contains(ItemFeature.ChargeAttack))
                powerMagnification *= 1.5f;
            if (applyChargeAttackMagnification && features.Contains(ItemFeature.SacrificeAttack))
                powerMagnification *= CommonSenseParameters.SacrificeAttackPowerMultiplier;
            return Mathf.RoundToInt(power * powerMagnification) + upgradeCount;
        }

        public static int GetSkillHpCost(List<ItemFeature> features) =>
            features.Contains(ItemFeature.SacrificeAttack) ? CommonSenseParameters.SacrificeAttackHpCost : 0;

        public static List<ElementPower> CreateElementPowers(int powerValue, List<ItemFeature> features)
        {
            var elementFeature = ElementFeatureMapping.Keys.FirstOrDefault(features.Contains);
            var elementPowers = new List<ElementPower>();
            if (elementFeature != default)
            {
                var element = ElementFeatureMapping[elementFeature];
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

        public static float GetCriticalRate(List<ItemFeature> features) =>
            features.Count(f => f == ItemFeature.Critical) * 0.25f;

        public static void AddCombatEffects(
            List<IEffect> effects,
            List<ItemFeature> features,
            List<ElementPower> elementPowers,
            float criticalRate,
            bool isWeaponAttack = true)
        {
            if (features.Contains(ItemFeature.Absorbing))
            {
                var absorbRate = features.Count(f => f == ItemFeature.Absorbing) * 0.25f;
                effects.Add(new AbsorbsEffect(elementPowers, absorbRate, criticalRate, isWeaponAttack));
            }
            else
            {
                effects.Add(new AttackEffect(elementPowers, criticalRate, isWeaponAttack));
            }

            if (features.Contains(ItemFeature.Knockback))
                effects.Add(new BlowAwayEffect(1));
            if (features.Contains(ItemFeature.Dig))
                effects.Add(new DigEffect());
            if (features.Contains(ItemFeature.BreakTrap))
                effects.Add(new BreakEffect(BreakTargets.Trap));

            var abnormalConditionMultiplier = features.Count(f => f == ItemFeature.EnhanceAbnormalCondition) + 1;
            foreach (var (feature, (templateName, baseProbability)) in ConditionFeatureMapping)
            {
                if (!features.Contains(feature))
                    continue;
                var probability = baseProbability * abnormalConditionMultiplier;
                var conditionData = new AdditionalConditionData(
                    ObjectLoader.Load<ConditionTemplate>(templateName), probability);
                effects.Add(new AddConditionEffect(conditionData));
            }
        }

        public static int GetAttackRepeatCount(List<ItemFeature> features)
        {
            if (features.Contains(ItemFeature.TripleAttack))
                return 3;
            if (features.Contains(ItemFeature.DoubleAttack))
                return 2;
            return 1;
        }

        public static float GetSkillOnUseProbabilityOfSuccess(List<ItemFeature> features)
        {
            if (features.Contains(ItemFeature.GuaranteedHit))
                return 1f;
            if (features.Contains(ItemFeature.Critical))
                return 0.75f;
            return CommonSenseParameters.SkillOnUseProbabilityOfSuccess;
        }

        public static float GetSkillOnThrowProbabilityOfSuccess(List<ItemFeature> features)
        {
            if (features.Contains(ItemFeature.GuaranteedHit))
                return 1f;
            if (features.Contains(ItemFeature.Critical))
                return 0.7f;
            return CommonSenseParameters.SkillOnThrowProbabilityOfSuccess;
        }

        public static int GetChargeTurn(List<ItemFeature> features) =>
            features.Contains(ItemFeature.ChargeAttack) ? 1 : 0;

        public static float GetMultiplyPrice(List<ItemFeature> features) =>
            features.Contains(ItemFeature.Artistic) ? 2f : 1f;

        public static float GetUsageLossChance(List<ItemFeature> features) =>
            1 - features.Count(f => f == ItemFeature.EnhanceDurability) * 0.2f;
    }
}
