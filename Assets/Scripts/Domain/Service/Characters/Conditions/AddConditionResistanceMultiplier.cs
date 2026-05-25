using Domain.Model.Character;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Sirenix.OdinInspector;
using Utilities;
using Utilities.Serialize;

namespace Domain.Service.Characters.Conditions
{
    internal class AddConditionResistanceMultiplier : IConditionData
    {
        public string Name => $"{Condition.Value.name}被付与確率(-{ResistanceRate:P0})";
        public ParticleType ParticleType => ParticleType.BloodRage;
        public Impact Impact => Impact.Beneficial;
        public ScriptableObjectSerializable<ConditionTemplate> Condition;
        [MinValue(0)] public float ResistanceRate = 0f;

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.GetConditionResistanceStat(Condition.Value).Add(ResistanceRate);
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.GetConditionResistanceStat(Condition.Value).Remove(ResistanceRate);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return Condition.Value.Evaluate(target) * ResistanceRate;
        }

        public float EvaluatePrice()
        {
            return Condition.Value.EvaluateDamage() * ResistanceRate;
        }
    }
}