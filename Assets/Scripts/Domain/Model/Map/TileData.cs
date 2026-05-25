using System;
using System.ComponentModel;
using Domain.Model.Memento;
using Utilities.WorldCreater;

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
            if (mapType == MapType.WorldMap)
                throw new ArgumentException("WorldMap cannot be used with TileCategory");
            return new TileMemento
            (
                mapType,
                (int)tileCategory,
                isKnown
            );
        }

        public static TileMemento Build(WorldTileType worldTileType, bool isKnown)
        {
            return new TileMemento
            (
                MapType.WorldMap,
                (int)worldTileType,
                isKnown
            );
        }

        public TileCategory Category()
        {
            if (MapType != MapType.WorldMap)
                return (TileCategory)Index;
            else
                return ((WorldTileType)Index).Category();
        }
        public bool IsWalkable() => Category().IsWalkable();
        public bool IsPassable() => Category().IsPassable();
        public bool IsTransparent() => Category().IsTransparent();
        public void SetKnown(bool isKnown)
        {
            IsKnown = isKnown;
        }
    }

    public static class WorldTileTypeExtension
    {
        public static TileCategory Category(this WorldTileType worldTileType) => worldTileType switch
        {
            WorldTileType.Blank => TileCategory.Blank,
            WorldTileType.Grass => TileCategory.Floor,
            WorldTileType.Ocean => TileCategory.Water,
            WorldTileType.Mountain => TileCategory.Wall,
            WorldTileType.Forest => TileCategory.Floor,
            WorldTileType.Desert => TileCategory.Floor,
            _ => throw new InvalidEnumArgumentException()
        };
    }
}