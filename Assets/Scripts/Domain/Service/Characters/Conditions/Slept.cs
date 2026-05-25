using Domain.Model.Character.Status;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Evaluation;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class Slept : IConditionData
    {
        public string Name => "睡眠";
        public ParticleType ParticleType => ParticleType.Sleep;
        public Impact Impact => Impact.Harmful;

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.GetFlagStat(FlagStatType.CannotAct).Add();
            hasCondition.Status.GetFlagStat(FlagStatType.Blind).Add();
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.GetFlagStat(FlagStatType.CannotAct).Remove();
            hasCondition.Status.GetFlagStat(FlagStatType.Blind).Remove();
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return target.Status.IsFlagStat(FlagStatType.CannotAct)
                ? 0
                : CommonSenseParameters.OneTurnStunEquivalentHpReduction;
        }

        public float EvaluatePrice()
        {
            return CommonSenseParameters.OneTurnStunEquivalentDamage;
        }
    }
}