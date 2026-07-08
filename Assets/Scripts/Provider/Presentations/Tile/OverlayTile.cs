#nullable enable
using System;
using Domain.Model.Map;
using UnityEngine;
using View.Map;
using View.Playback.Values;

namespace Provider.Presentations.Tile
{
    internal sealed class OverlayTile
    {
        private readonly Vector2Int _position;
        private readonly OverlayTileCategory _category;

        public OverlayTile(Vector2Int position, OverlayTileCategory category)
        {
            _position = position;
            _category = category;
        }

        public OverlayAppearance AppearanceWith(TileSet set, TileVisibility visibility)
        {
            return new OverlayAppearance(_position, _category switch
            {
                OverlayTileCategory.Grass => OverlayKind.Grass,
                OverlayTileCategory.FloatingIce => OverlayKind.FloatingIce,
                _ => throw new ArgumentOutOfRangeException(nameof(_category), _category, null)
            }, set, visibility);
        }
    }
}
