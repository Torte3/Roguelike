using Domain.Model.Effect;
using Domain.Model.Entity;
using Utilities;

namespace Domain.Model.Condition
{
    public interface IConditionData
    {
        public string Name { get; }
        public ParticleType ParticleType { get; }
        public Impact Impact { get; }
        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor);
        public void Delete(IHasCondition hasCondition, Id<IEntity> actor);
        public float Evaluate(ITargetOfEffect target);
        public float EvaluatePrice();
    }
}