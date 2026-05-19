using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Condition;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Logs;
using ObservableCollections;
using R3;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class CharacterConditions : IDisposable
    {
        private readonly ObservableHashSet<ICondition> _conditions = new();
        private readonly Dictionary<ICondition, Id<IEntity>> _inflicterMap = new();
        private readonly CompositeDisposable _disposables = new();

        public CharacterConditions(IHasCondition hasCondition,
            List<(Id<IEntity> actor, ConditionMemento condition)> conditions, IMap map)
        {
            foreach (var (actor, conditionMemento) in conditions)
            {
                var condition = new Condition(conditionMemento);
                _conditions.Add(condition);
                _inflicterMap.Add(condition, actor);
            }

            _conditions.ObserveAdd()
                .Subscribe(add =>
                {
                    if (!_conditions.Any(condition =>
                            !ReferenceEquals(condition, add.Value) && condition.IsEqualCondition(add.Value)))
                    {
                        var inflictLog = add.Value.GetInflictLog(hasCondition, map.Player);
                        if (inflictLog != null)
                        {
                            GameLog.Add(hasCondition.IsVisible, inflictLog);
                        }
                    }
                    add.Value.Inflict(hasCondition, _inflicterMap[add.Value]);
                })
                .AddTo(_disposables);
            _conditions.ObserveRemove()
                .Subscribe(remove =>
                {
                    if (!_conditions.Any(condition => condition.IsEqualCondition(remove.Value)))
                    {
                        var deleteLog = remove.Value.GetDeleteLog(hasCondition, map.Player);
                        if (deleteLog != null)
                        {
                            GameLog.Add(hasCondition.IsVisible, deleteLog);
                        }
                    }
                    remove.Value.Delete(hasCondition, _inflicterMap[remove.Value]);
                })
                .AddTo(_disposables);
        }

        public IObservableCollection<ICondition> Conditions => _conditions;

        public List<(Id<IEntity> actor, ICondition condition)> ConditionsWithInflicter =>
            _conditions.Select(condition => (_inflicterMap[condition], condition)).ToList();

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void Add(Id<IEntity> actor, ConditionTemplate conditionData)
        {
            var condition = new Condition(Condition.Build(conditionData));
            _inflicterMap.Add(condition, actor);
            _conditions.Add(condition);
        }

        public void RemoveType(Type conditionType)
        {
            var removedConditions =
                _conditions.Where(condition => condition.EqualsConditionType(conditionType)).ToList();
            foreach (var condition in removedConditions)
            {
                _conditions.Remove(condition);
                _inflicterMap.Remove(condition);
            }
        }

        public void Clear()
        {
            foreach (var condition in _conditions.ToList())
            {
                _conditions.Remove(condition);
                _inflicterMap.Remove(condition);
            }
        }

        public void UpdateTurn(bool characterVisible)
        {
            var removedConditions = _conditions.Where(condition => condition.ShouldDelete(characterVisible)).ToList();
            foreach (var condition in removedConditions)
            {
                _conditions.Remove(condition);
                _inflicterMap.Remove(condition);
            }
            foreach (var condition in _conditions)
            {
                condition.UpdateTurn();
            }
        }

        public void WasAttacked()
        {
            _conditions.RemoveRange(_conditions.Where(condition => condition.ShouldDeleteByDamage()).ToList());
        }
    }
}