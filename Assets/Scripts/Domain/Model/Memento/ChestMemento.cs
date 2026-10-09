#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Entity;
using Domain.Model.Map;
using UnityEngine;
using Utilities;
using Utilities.Serialize;
using Utilities.Serialize.Option;

namespace Domain.Model.Memento
{

    [Serializable]
    public class ChestMemento
    {
        [field: SerializeReference] public List<IItemMemento> Items { get; private set; }
        [SerializeField] private Option<ScriptableObjectSerializable<EnemyData>> _mimic;
        public Option<EnemyData> Mimic => _mimic.Map(m => m.Value);
        [field: SerializeField] public EntityMemento Entity { get; private set; }
        [field: SerializeField] public bool HasLock { get; private set; }
        [SerializeField] private List<string> _keyHolders;
        public List<Id<IEntity>> KeyHolders => _keyHolders.Select(keyHolder => new Id<IEntity>(keyHolder)).ToList();

        public ChestMemento(
            List<IItemMemento> items,
            Option<EnemyData> mimic,
            EntityMemento entity,
            bool hasLock,
            IEnumerable<Id<IEntity>> keyHolders)
        {
            Items = items;
            _mimic = mimic.Map(m => m.ToSerializable());
            Entity = entity;
            HasLock = hasLock;
            _keyHolders = keyHolders.Select(keyHolder => keyHolder.ToString()).ToList();
        }

        public ChestMemento(List<IItemMemento> items, EntityMemento entity) : this(
            items,
            Option.None<EnemyData>(),
            entity,
            false,
            new List<Id<IEntity>>())
        {
        }

        public ChestMemento(IItemMemento item, EntityMemento entity) : this(
            new List<IItemMemento> { item },
            Option.None<EnemyData>(),
            entity,
            false,
            new List<Id<IEntity>>())
        {
        }

        public ChestMemento(EnemyData mimic, EntityMemento entity) : this(
            new List<IItemMemento>(),
            Option.Some(mimic),
            entity,
            false,
            new List<Id<IEntity>>())
        {
        }
    }
}