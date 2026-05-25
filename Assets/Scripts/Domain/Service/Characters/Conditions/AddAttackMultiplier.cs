using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Evaluation;
using Sirenix.OdinInspector;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class AddAttackMultiplier : IConditionData
    {
        public string Name => $"{Element.Name()}攻撃倍率(+{AddedMultiplier:P0})";
        public ParticleType ParticleType => ParticleType.BloodRage;
        public Impact Impact => Impact.Beneficial;
        public Element Element;
        [MinValue(0)] public float AddedMultiplier = 0f;

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.GetElementAttackMultiplierStat(Element).Add(AddedMultiplier);
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.GetElementAttackMultiplierStat(Element).Remove(AddedMultiplier);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return CommonSenseParameters.AttacksPerTurn * CommonSenseParameters.HpReductionPerTurn * AddedMultiplier;
        }

        public float EvaluatePrice()
        {
            return CommonSenseParameters.AttacksPerTurn * CommonSenseParameters.DamagePerAttack * AddedMultiplier;
        }
    }
}