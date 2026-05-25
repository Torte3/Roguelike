using Domain.Model.Character.Status;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Evaluation;
using Sirenix.OdinInspector;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class Poisoned : IConditionData
    {
        public string Name => $"毒({Power})";
        public ParticleType ParticleType => ParticleType.PoisoningBubble;
        public Impact Impact => Impact.Harmful;
        [MinValue(0)] public float Power = 1;

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.GetStat(StatType.HpNaturalRecovery).Remove(Power);
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.GetStat(StatType.HpNaturalRecovery).Add(Power);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return Power / CommonSenseParameters.PlayerMaxHealth;
        }

        public float EvaluatePrice()
        {
            return Power;
        }
    }
}