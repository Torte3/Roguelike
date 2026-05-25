using System;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class ItemEntityMemento
    {
        [field: SerializeReference] public IItemMemento Item { get; private set; }
        [field: SerializeField] public EntityMemento Entity { get; private set; }

        public ItemEntityMemento(IItemMemento item, EntityMemento entity)
        {
            Item = item;
            Entity = entity;
        }
    }
}