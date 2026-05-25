#nullable enable
using System;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class BonfireMemento
    {
        [field: SerializeField] public int RemainingUsages { get; private set; }
        [field: SerializeField] public EntityMemento Entity { get; private set; }

        public BonfireMemento(int remainingUsages, EntityMemento entity)
        {
            RemainingUsages = remainingUsages;
            Entity = entity;
        }
    }
}