using System;
using UnityEngine;
using Utilities;

namespace Domain.Model.Map
{
    [Serializable]
    public class MapConnection
    {
        [field: SerializeField] public MovementEntityType Type { get; private set; }
        [field: SerializeField] public Id<IMap> Destination { get; private set; }

        public MapConnection(MovementEntityType type, Id<IMap> destination)
        {
            Type = type;
            Destination = destination;
        }
    }
}