#nullable enable
using System;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class MagicPotMemento
    {
        [field: SerializeField] public int RemainingUsages { get; private set; }
        [field: SerializeField] public EntityMemento Entity { get; private set; }

        public MagicPotMemento(int remainingUsages, EntityMemento entity)
        {
            RemainingUsages = remainingUsages;
            Entity = entity;
        }
    }
}