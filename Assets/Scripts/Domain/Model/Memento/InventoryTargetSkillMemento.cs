using System;
using Domain.Model.Item;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class InventoryTargetSkillMemento : ISkillMemento
    {
        [field: SerializeReference] public IInventoryEffect InventoryEffect { get; private set; }

        public InventoryTargetSkillMemento(IInventoryEffect inventoryEffect)
        {
            InventoryEffect = inventoryEffect;
        }
    }
}