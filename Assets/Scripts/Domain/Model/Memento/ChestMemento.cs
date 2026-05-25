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
        [SerializeField] private List<string> _keyCharacters;
        public List<Id<IEntity>> KeyCharacters => _keyCharacters.Select(keyCharacter => new Id<IEntity>(keyCharacter)).ToList();

        public ChestMemento(
            List<IItemMemento> items,
            Option<EnemyData> mimic,
            EntityMemento entity,
            List<Id<IEntity>> keyCharacters)
        {
            Items = items;
            _mimic = mimic.Map(m => m.ToSerializable());
            Entity = entity;
            _keyCharacters = keyCharacters.Select(keyCharacter => keyCharacter.ToString()).ToList();
        }

        public ChestMemento(List<IItemMemento> items, EntityMemento entity) : this(
            items,
            Option.None<EnemyData>(),
            entity,
            new List<Id<IEntity>>())
        {
        }

        public ChestMemento(IItemMemento item, EntityMemento entity) : this(
            new List<IItemMemento> { item },
            Option.None<EnemyData>(),
            entity,
            new List<Id<IEntity>>())
        {
        }

        public ChestMemento(EnemyData mimic, EntityMemento entity) : this(
            new List<IItemMemento>(),
            Option.Some(mimic),
            entity,
            new List<Id<IEntity>>())
        {
        }
    }
}