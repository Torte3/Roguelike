using Domain.Model.Character.Status;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class FlagCondition : IConditionData
    {
        [Required, SerializeField] private StringSerializableFlagStatType _flagStatType;
        public string Name => _flagStatType.Value.GetName();
        public ParticleType ParticleType => _flagStatType.Value.GetParticleType();
        public Impact Impact => _flagStatType.Value.GetImpact();

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.GetFlagStat(_flagStatType.Value).Add();
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.GetFlagStat(_flagStatType.Value).Remove();
        }

        public float Evaluate(ITargetOfEffect target)
        {
            if (!target.Status.GetFlagStat(_flagStatType.Value).CurrentValue)
            {
                return _flagStatType.Value.Evaluate(target);
            }
            return 0;
        }
        public float EvaluatePrice() => _flagStatType.Value.EvaluatePrice();
    }
}