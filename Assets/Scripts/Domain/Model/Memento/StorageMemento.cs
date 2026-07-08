using System;
using System.Collections.Generic;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class StorageMemento
    {
        [field: SerializeField] public int Capacity { get; private set; }
        [field: SerializeReference] public List<IItemMemento> Items { get; private set; }
        [field: SerializeField] public bool CanAddItem { get; private set; }
        [field: SerializeField] public bool CanRemoveItem { get; private set; }

        public StorageMemento(int capacity, List<IItemMemento> items, bool canAddItem, bool canRemoveItem)
        {
            Capacity = capacity;
            Items = items;
            CanAddItem = canAddItem;
            CanRemoveItem = canRemoveItem;
        }
    }
}