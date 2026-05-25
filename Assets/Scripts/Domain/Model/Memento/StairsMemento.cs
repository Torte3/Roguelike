#nullable enable
using System;
using Domain.Model.Entity;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Model.Memento
{
    [Serializable]
    public class StairsMemento
    {
        [field: SerializeField] public MovementEntityType Type { get; private set; }
        [SerializeField] private string _destination;
        public Id<IMap> Destination => new(_destination);
        [SerializeField] private string _destinationId;
        public Id<IEntity> DestinationId => new(_destinationId);
        [field: SerializeField] public EntityMemento Entity { get; private set; }
        [field: SerializeField] public bool IsUsed { get; private set; }

        public StairsMemento(
            MovementEntityType type,
            Id<IMap> destination,
            Id<IEntity> destinationId,
            EntityMemento entity,
            bool isUsed)
        {
            Type = type;
            _destination = destination.ToString();
            _destinationId = destinationId.ToString();
            Entity = entity;
            IsUsed = isUsed;
        }
    }
}
