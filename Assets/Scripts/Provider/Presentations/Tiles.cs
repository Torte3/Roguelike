#nullable enable
using System;
using Domain.Model.Map;
using Domain.Model.WorldEvents;
using UnityEngine;
using View;
using View.Playback;

namespace Provider.Presentations
{
    internal static class Tiles
    {
        public static TileSet SetOf(MapType mapType)
        {
            return mapType switch
            {
                MapType.Cave => TileSet.Cave,
                MapType.Forest => TileSet.Forest,
                MapType.Snow => TileSet.Snow,
                MapType.Volcano => TileSet.Volcano,
                MapType.Desert => TileSet.Desert,
                MapType.Dungeon => TileSet.Dungeon,
                MapType.Void => TileSet.Void,
                _ => throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null)
            };
        }

        public static TileSpec Of(TileState tile)
        {
            return new TileSpec(tile.Position, SetOf(tile.MapType), KindOf(tile.Category), tile.IsKnown);
        }

        public static OverlaySpec Of(Vector2Int position, OverlayTileCategory? category)
        {
            return new OverlaySpec(position, category switch
            {
                OverlayTileCategory.Grass => OverlayKind.Grass,
                OverlayTileCategory.FloatingIce => OverlayKind.FloatingIce,
                null => null,
                _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
            });
        }

        private static TileKind KindOf(TileCategory category)
        {
            return category switch
            {
                TileCategory.Floor => TileKind.Floor,
                TileCategory.Water => TileKind.Water,
                TileCategory.Wall => TileKind.Wall,
                TileCategory.UnbreakableWall => TileKind.UnbreakableWall,
                _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
            };
        }
    }
}
