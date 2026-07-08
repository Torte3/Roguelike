#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Map;
using Domain.Model.WorldEvents;
using Provider.Presentations.Translation;
using UnityEngine;
using View.Map;
using View.Playback.Values;

namespace Provider.Presentations.Tile
{
    public sealed class TileVisibilities
    {
        private readonly HashSet<Vector2Int> _known = new();
        private readonly PlayerSight _sight;
        private TileSet _set;

        public TileVisibilities(PlayerSight sight)
        {
            _sight = sight;
        }

        internal IReadOnlyList<PositionedTileVisibility> MapEntered(TileSet set, IReadOnlyList<TileState> tiles)
        {
            _set = set;
            _known.Clear();
            return TilesChanged(tiles);
        }

        internal IReadOnlyList<PositionedTileVisibility> TilesChanged(IReadOnlyList<TileState> tiles)
        {
            foreach (var tile in tiles)
                SetKnown(tile.Position, tile.IsKnown);
            return Of(tiles.Select(tile => tile.Position));
        }

        internal IReadOnlyList<PositionedTileVisibility> Known(IReadOnlyList<Vector2Int> positions)
        {
            _known.UnionWith(positions);
            return Of(positions);
        }

        internal IReadOnlyList<PositionedTileVisibility> Forgotten(IReadOnlyList<Vector2Int> positions)
        {
            _known.ExceptWith(positions);
            return Of(positions);
        }

        internal IReadOnlyList<PositionedTileVisibility> Of(IEnumerable<Vector2Int> positions)
        {
            return positions.Select(position => new PositionedTileVisibility(position, VisibilityAt(position))).ToList();
        }

        internal IReadOnlyList<OverlayAppearance> OverlaysOf(
            IEnumerable<(Vector2Int Position, OverlayTileCategory Category)> overlays)
        {
            return overlays.Select(overlay => new OverlayTile(overlay.Position, overlay.Category).AppearanceWith(_set, VisibilityAt(overlay.Position)))
                .ToList();
        }

        private void SetKnown(Vector2Int position, bool isKnown)
        {
            if (isKnown)
                _known.Add(position);
            else
                _known.Remove(position);
        }

        private TileVisibility VisibilityAt(Vector2Int position)
        {
            if (!_known.Contains(position))
                return TileVisibility.Transparent;
            return _sight.Contains(position) ? TileVisibility.Visible : TileVisibility.Translucent;
        }
    }
}
