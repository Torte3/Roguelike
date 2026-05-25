using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Effect;
using Domain.Model.Evaluation;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    internal static class Formula
    {
        public static int Calc(IActorOfEffect actor, ITargetOfEffect target, List<ElementPower> powers,
            bool isCritical = false)
        {
            if (target.Status.IsFlagStat(FlagStatType.Hard) && !isCritical)
                return 1;

            var elementDamages = new List<float>();
            foreach (var elementPower in powers)
            {
                var attackMultiplier = actor.Status.GetCombinedElementAttackMultiplier(elementPower.Element);
                var elementResistanceMultiplier = target.Status.GetElementDamageRateMultiplier(elementPower.Element);
                elementDamages.Add(elementPower.Power * attackMultiplier * elementResistanceMultiplier);
            }

            var damage = elementDamages.Sum() * (isCritical ? 2 : 1);
            if (target.Status.IsFlagStat(FlagStatType.AdjacentAttackGuard) && IsAdjacentAttack(actor, target))
                damage *= CommonSenseParameters.AdjacentDamageMultiplier;

            return Mathf.Max(1, Mathf.RoundToInt(damage));
        }

        private static bool IsAdjacentAttack(IActorOfEffect actor, ITargetOfEffect target)
        {
            return VectorExtension.ChebyshevDistance(
                       actor.Entity.CurrentPosition,
                       target.Entity.CurrentPosition) <= 1;
        }

        public static int EvaluateDamage(List<ElementPower> powers, bool isCritical = false)
        {
            return Mathf.RoundToInt(powers.Sum(power => power.Power) * (isCritical ? 2 : 1));
        }

        public static int CalcHeal(int power)
        {
            return Mathf.RoundToInt(power);
        }

        public static int EvaluateHeal(int power)
        {
            return Mathf.RoundToInt(power);
        }

        public static int CalcExplosionDamage(float damageRate, ITargetOfEffect target)
        {
            var damage = Mathf.Max(1, Mathf.RoundToInt(target.CurrentMaxHp * damageRate));
            if (target.Status.IsFlagStat(FlagStatType.ExplosionProof))
                return Mathf.Min(CommonSenseParameters.DamageWhenExplosionProof, damage);
            return damage;
        }

        public static int EvaluateExplosionDamage(float damageRate)
        {
            return Mathf.RoundToInt(CommonSenseParameters.PlayerMaxHealth * damageRate);
        }
    }
}