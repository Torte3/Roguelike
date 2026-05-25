#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character.Status;
using Domain.Model.Entity;
using UnityEngine;
using Utilities;
using Utilities.Serialize;

namespace Domain.Model.Memento
{
    [Serializable]
    public class CharacterStatusMemento
    {
        [field: SerializeField] public CharacterStatsMemento Stats { get; private set; }
        [SerializeField] private SerializableDictionary<FlagStatType, int> _flagStats;
        public Dictionary<FlagStatType, int> FlagStats => _flagStats.ToDictionary();
        [SerializeField] private List<ConditionMemento> _conditions;
        [SerializeField] private List<string> _inflicters;

        public List<(Id<IEntity> actor, ConditionMemento condition)> Conditions =>
            _conditions.Select((x, i) => (new Id<IEntity>(_inflicters[i]), x)).ToList();

        public CharacterStatusMemento(
            CharacterStatsMemento stats,
            Dictionary<FlagStatType, int> flagStats,
            List<(Id<IEntity> actor, ConditionMemento condition)> conditions)
        {
            Stats = stats;
            _flagStats = flagStats.ToSerializable();
            _conditions = conditions.Select(x => x.condition).ToList();
            _inflicters = conditions.Select(x => x.actor.ToString()).ToList();
        }

        public CharacterStatusMemento CopyWith(
            CharacterStatsMemento? stats = null,
            Dictionary<FlagStatType, int>? flagStats = null,
            List<(Id<IEntity> actor, ConditionMemento condition)>? conditions = null)
        {
            return new CharacterStatusMemento(stats ?? Stats, flagStats ?? FlagStats, conditions ?? Conditions);
        }
    }
}