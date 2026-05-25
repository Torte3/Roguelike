using Domain.Model.Character.Status;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Sirenix.OdinInspector;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class AddMaxHp : IConditionData
    {
        public string Name => $"最大HP(+{AddValue})";
        public ParticleType ParticleType => ParticleType.None;
        public Impact Impact => Impact.Beneficial;
        [MinValue(0)] public int AddValue;

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.GetStat(StatType.MaxHp).Add(AddValue);
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.GetStat(StatType.MaxHp).Remove(AddValue);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return AddValue / target.Status.GetStatValue(StatType.MaxHp);
        }

        public float EvaluatePrice()
        {
            return AddValue;
        }
    }
}