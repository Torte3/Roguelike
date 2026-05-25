using System;
using Domain.Model.Character;
using Domain.Model.Condition;
using Domain.Model.Entity;
using Domain.Model.Memento;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class Condition : ICondition
    {
        public ConditionTemplate _condition { get; init; }
        private int _elapsedTurn;

        public Condition(ConditionMemento memento)
        {
            _elapsedTurn = memento.ElapsedTurns;
            _condition = memento.Condition;
        }

        public bool IsEqualCondition(ICondition condition)
        {
            if (condition is Condition otherCondition)
            {
                return _condition.name == otherCondition._condition.name;
            }
            return false;
        }

        public ParticleType ParticleType => _condition.Condition.ParticleType;

        public ConditionMemento Serialize()
        {
            return new ConditionMemento
            (
                _condition,
                _elapsedTurn
            );
        }

        public ConditionMemento Build(ConditionTemplate condition, int elapsedTurn = 0)
        {
            return new ConditionMemento
            (
                condition,
                elapsedTurn
            );
        }

        public string? GetInflictLog(IHasCondition hasCondition, IPlayer player)
        {
            if (string.IsNullOrEmpty(_condition.InflictLog))
            {
                return null;
            }
            return $"{hasCondition.GetName(player)}{_condition.InflictLog}";
        }

        public string? GetDeleteLog(IHasCondition hasCondition, IPlayer player)
        {
            if (string.IsNullOrEmpty(_condition.DeleteLog))
            {
                return null;
            }
            return $"{hasCondition.GetName(player)}{_condition.DeleteLog}";
        }

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            _condition.Condition.Inflict(hasCondition, actor);
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            _condition.Condition.Delete(hasCondition, actor);
        }

        public void UpdateTurn()
        {
            _elapsedTurn += 1;
        }

        public bool ShouldDelete(bool characterVisible)
        {
            return _condition.RemovalCondition.IsFinished(_elapsedTurn, characterVisible);
        }

        public bool ShouldDeleteByDamage()
        {
            return _condition.RemovalCondition.IsFinishedByDamage();
        }

        public bool EqualsConditionType(Type conditionType)
        {
            return _condition.GetType() == conditionType;
        }

        public static ConditionMemento Build(ConditionTemplate condition)
        {
            return new ConditionMemento
            (
                condition,
                0
            );
        }

        public string Info()
        {
            return $"{_condition.Info(_elapsedTurn)}";
        }
    }
}