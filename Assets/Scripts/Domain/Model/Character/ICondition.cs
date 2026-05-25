#nullable enable
using Domain.Model.Condition;
using Domain.Model.Entity;
using Domain.Model.Memento;
using Utilities;

namespace Domain.Model.Character
{
    public interface ICondition : IHasInfo, ISerializable<ConditionMemento>
    {
        public ParticleType ParticleType { get; }
        public bool IsEqualCondition(ICondition condition);
        public string? GetInflictLog(IHasCondition hasCondition, IPlayer player);
        public string? GetDeleteLog(IHasCondition hasCondition, IPlayer player);
        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor);
        public void Delete(IHasCondition hasCondition, Id<IEntity> actor);
        public void UpdateTurn();
        public bool ShouldDelete(bool characterVisible);
        public bool ShouldDeleteByDamage();
        public bool EqualsConditionType(System.Type conditionType);
    }
}