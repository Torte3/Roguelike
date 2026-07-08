using Domain.Model.Memento;
using Domain.Model.WorldEvents;
using UnityEngine;

namespace Domain.Model.Map
{
    public class TileData : ISerializable<TileMemento>
    {
        public readonly MapType MapType;
        public readonly int Index;
        public bool IsKnown { get; private set; }

        public TileData(TileMemento memento)
        {
            MapType = memento.MapType;
            Index = memento.Index;
            IsKnown = memento.IsKnown;
        }

        public TileMemento Serialize()
        {
            return new TileMemento
            (
                MapType,
                Index,
                IsKnown
            );
        }

        public static TileMemento Build(MapType mapType, TileCategory tileCategory, bool isKnown)
        {
            return new TileMemento
            (
                mapType,
                (int)tileCategory,
                isKnown
            );
        }

        public TileCategory Category()
        {
            return (TileCategory)Index;
        }
        public TileState ToState(Vector2Int position)
        {
            return new TileState(position, MapType, Category(), IsKnown);
        }

        public bool IsWalkable() => Category().IsWalkable();
        public bool IsPassable() => Category().IsPassable();
        public bool IsTransparent() => Category().IsTransparent();
        public void SetKnown(bool isKnown)
        {
            IsKnown = isKnown;
        }
    }
}