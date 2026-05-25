#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class EntitiesMemento
    {
        [field: SerializeField] public List<CharacterMemento> Characters { get; private set; }
        [field: SerializeField] public List<ItemEntityMemento> Items { get; private set; }
        [field: SerializeField] public EventEntitiesMemento EventEntities { get; private set; }
        [field: SerializeField] public FireEntitiesMemento Fires { get; private set; }
        public EntitiesMemento(
            List<CharacterMemento> characters,
            List<ItemEntityMemento> items,
            EventEntitiesMemento eventEntities,
            FireEntitiesMemento fires)
        {
            Characters = characters;
            Items = items;
            EventEntities = eventEntities;
            Fires = fires;
        }
    }
}