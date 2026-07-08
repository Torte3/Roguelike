#nullable enable
using System;
using Domain.Model.Map;
using Domain.Model.WorldEvents;
using View.Map;

namespace Provider.Presentations.Tile
{
    internal sealed class MapTiles
    {
        private readonly MapType _type;

        public MapTiles(MapType type)
        {
            _type = type;
        }

        public TileSet Set => _type switch
        {
            MapType.Cave => TileSet.Cave,
            MapType.Forest => TileSet.Forest,
            MapType.Snow => TileSet.Snow,
            MapType.Volcano => TileSet.Volcano,
            MapType.Desert => TileSet.Desert,
            MapType.Dungeon => TileSet.Dungeon,
            MapType.Void => TileSet.Void,
            _ => throw new ArgumentOutOfRangeException(nameof(_type), _type, null)
        };
    }
}
