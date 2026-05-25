#nullable enable
using System;
using Domain.Model.Map;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class TileMemento
    {
        [field: SerializeField] public MapType MapType { get; private set; }
        [field: SerializeField] public int Index { get; private set; }
        [field: SerializeField] public bool IsKnown { get; private set; }

        public TileMemento(MapType mapType, int index, bool isKnown)
        {
            MapType = mapType;
            Index = index;
            IsKnown = isKnown;
        }
    }
}