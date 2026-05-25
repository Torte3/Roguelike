using Domain.Model.Character.Status;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Sirenix.OdinInspector;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class NaturalHeal : IConditionData
    {
        public string Name => $"自然治癒({Power})";
        public ParticleType ParticleType => ParticleType.HealGreen;
        public Impact Impact => Impact.Beneficial;
        [MinValue(0)] public float Power;

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.GetStat(StatType.HpNaturalRecovery).Add(Power);
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.GetStat(StatType.HpNaturalRecovery).Remove(Power);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return Power / target.CurrentMaxHp / 2f;
        }

        public float EvaluatePrice()
        {
            return Power;
        }
    }
}