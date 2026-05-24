using Domain.Model.Character.Status;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Evaluation;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class Acceleration : IConditionData
    {
        public string Name => "加速";
        public ParticleType ParticleType => ParticleType.FastSpeed;
        public Impact Impact => Impact.Beneficial;

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.GetStat(StatType.MaxWaitTime).AddDivisor(1);
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.GetStat(StatType.MaxWaitTime).RemoveDivisor(1);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return 0.07f;
        }

        public float EvaluatePrice()
        {
            return CommonSenseParameters.AttacksPerTurn * CommonSenseParameters.DamagePerAttack;
        }
    }
}