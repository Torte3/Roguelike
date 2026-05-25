using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Evaluation;
using Sirenix.OdinInspector;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class AddResistanceMultiplier : IConditionData
    {
        public string Name => $"{Element}被ダメージ倍率(-{AddedResistanceMultiplier:P0})";
        public ParticleType ParticleType => ParticleType.BloodRage;
        public Impact Impact => Impact.Beneficial;
        public Element Element;
        [MinValue(0)] public float AddedResistanceMultiplier = 0f;

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.GetElementDamageRateMultiplierStat(Element).Remove(AddedResistanceMultiplier);
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.GetElementDamageRateMultiplierStat(Element).Add(AddedResistanceMultiplier);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return CommonSenseParameters.AttacksPerTurn * CommonSenseParameters.HpReductionPerTurn * AddedResistanceMultiplier;
        }

        public float EvaluatePrice()
        {
            return CommonSenseParameters.AttacksPerTurn * CommonSenseParameters.DamagePerAttack * AddedResistanceMultiplier;
        }
    }
}