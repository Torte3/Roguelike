#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Map;
using Domain.Model.WorldEvents;
using UnityEngine;
using View.Playback.Values;

namespace Provider.Presentations.Tile
{
    public sealed class TileKindClassifier
    {
        private static readonly IReadOnlyDictionary<TileCategory, TileKind> ShopTileKinds =
            new Dictionary<TileCategory, TileKind> { { TileCategory.Floor, TileKind.ShopFloor } };

        private readonly List<(RectInt Area, IReadOnlyDictionary<TileCategory, TileKind> TileKinds)> _rooms = new();
        internal IReadOnlyList<TileAppearance> MapEntered(RectInt? shopRect, IReadOnlyList<TileState> tiles)
        {
            _rooms.Clear();
            if (shopRect.HasValue)
                _rooms.Add((shopRect.Value, ShopTileKinds));
            return AppearancesOf(tiles);
        }

        internal IReadOnlyList<TileAppearance> TilesChanged(IReadOnlyList<TileState> tiles)
        {
            return AppearancesOf(tiles);
        }

        private IReadOnlyList<TileAppearance> AppearancesOf(IReadOnlyList<TileState> tiles)
        {
            return tiles.Select(tile => KindOf(tile).AppearanceAt(tile)).ToList();
        }

        private TileKind KindOf(TileState tile)
        {
            foreach (var room in _rooms)
            {
                if (room.Area.Contains(tile.Position) && room.TileKinds.TryGetValue(tile.Category, out var kind))
                    return kind;
            }

            return KindOf(tile.Category);
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
