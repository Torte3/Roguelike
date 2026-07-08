using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Condition;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.WorldEvents;
using ObservableCollections;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class CharacterConditions
    {
        private readonly ObservableHashSet<ICondition> _conditions = new();
        private readonly Dictionary<ICondition, Id<IEntity>> _inflicterMap = new();
        private readonly IHasCondition _hasCondition;
        private readonly IMap _map;

        public CharacterConditions(IHasCondition hasCondition,
            List<(Id<IEntity> actor, ConditionMemento condition)> conditions, IMap map)
        {
            _hasCondition = hasCondition;
            _map = map;
            foreach (var (actor, conditionMemento) in conditions)
            {
                var condition = new Condition(conditionMemento);
                _conditions.Add(condition);
                _inflicterMap.Add(condition, actor);
            }
        }

        public IObservableCollection<ICondition> Conditions => _conditions;

        public IReadOnlyList<ParticleType> Particles => _conditions
            .Select(condition => condition.ParticleType)
            .Where(particle => particle != ParticleType.None)
            .Distinct()
            .OrderBy(particle => particle)
            .ToList();

        public List<(Id<IEntity> actor, ICondition condition)> ConditionsWithInflicter =>
            _conditions.Select(condition => (_inflicterMap[condition], condition)).ToList();

        public void Add(Id<IEntity> actor, ConditionTemplate conditionData)
        {
            var condition = new Condition(Condition.Build(conditionData));
            var isNew = !_conditions.Any(existing => existing.IsEqualCondition(condition));
            _inflicterMap.Add(condition, actor);
            _conditions.Add(condition);
            _map.Events.Record(new ConditionInflicted(_hasCondition.Entity.Ref, _hasCondition.Label,
                isNew ? condition.InflictLog : null, Particles, _map.ShopLookIn()));
            condition.Inflict(_hasCondition, actor);
        }

        public void RemoveType(Type conditionType)
        {
            RemoveAll(_conditions.Where(condition => condition.EqualsConditionType(conditionType)).ToList());
        }

        public void Clear()
        {
            RemoveAll(_conditions.ToList());
        }

        public void UpdateTurn(bool characterVisible)
        {
            RemoveAll(_conditions.Where(condition => condition.ShouldDelete(characterVisible)).ToList());
            foreach (var condition in _conditions)
            {
                condition.UpdateTurn();
            }
        }

        public void WasAttacked()
        {
            RemoveAll(_conditions.Where(condition => condition.ShouldDeleteByDamage()).ToList());
        }

        private void RemoveAll(IEnumerable<ICondition> conditions)
        {
            foreach (var condition in conditions)
                Remove(condition);
        }

        private void Remove(ICondition condition)
        {
            var actor = _inflicterMap[condition];
            _conditions.Remove(condition);
            _inflicterMap.Remove(condition);
            var isLast = !_conditions.Any(remaining => remaining.IsEqualCondition(condition));
            _map.Events.Record(new ConditionRemoved(_hasCondition.Entity.Ref, _hasCondition.Label,
                isLast ? condition.DeleteLog : null, Particles, _map.ShopLookIn()));
            condition.Delete(_hasCondition, actor);
        }
    }
}