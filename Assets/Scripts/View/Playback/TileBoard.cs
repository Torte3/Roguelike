#nullable enable
using System;
using System.Collections.Generic;
using R3;
using UnityEngine;
using VContainer;

namespace View.Playback
{
    public sealed class TileBoard
    {
        private readonly TileViewController _tileView;
        private readonly OverlayTileViewController _overlayTileView;
        private readonly MinimapController _minimapController;
        private readonly TilePalette _tilePalette;
        private readonly ShownSight _sight;
        private readonly Dictionary<Vector2Int, TileSet> _tileSets = new();
        private readonly HashSet<Vector2Int> _knownPositions = new();
        private TileSet _defaultTileSet;
        private RectInt? _shopRect;

        [Inject]
        public TileBoard(TileViewController tileView, OverlayTileViewController overlayTileView,
            MinimapController minimapController, TilePalette tilePalette, ShownSight sight)
        {
            _tileView = tileView;
            _overlayTileView = overlayTileView;
            _minimapController = minimapController;
            _tilePalette = tilePalette;
            _sight = sight;

            sight.OnChanged.Subscribe(changed =>
            {
                foreach (var position in changed)
                    SetVisibility(position, VisibilityAt(position));
            });
        }

        internal void Enter(TileSet defaultTileSet, RectInt? shopRect, IReadOnlyList<TileSpec> tiles,
            IReadOnlyList<OverlaySpec> overlays)
        {
            _tileView.Clear();
            _overlayTileView.Clear();
            _minimapController.Clear();
            _tileSets.Clear();
            _knownPositions.Clear();
            _defaultTileSet = defaultTileSet;
            _shopRect = shopRect;

            foreach (var tile in tiles)
                ApplyTile(tile);

            foreach (var overlay in overlays)
                ApplyOverlay(overlay);

            foreach (var tile in tiles)
                SetVisibility(tile.Position, VisibilityAt(tile.Position));
        }

        internal void Change(IReadOnlyList<TileSpec> tiles)
        {
            foreach (var tile in tiles)
            {
                ApplyTile(tile);
                SetVisibility(tile.Position, VisibilityAt(tile.Position));
            }
        }

        internal void ChangeOverlays(IReadOnlyList<OverlaySpec> overlays)
        {
            foreach (var overlay in overlays)
                ApplyOverlay(overlay);
        }

        internal void SetKnown(IReadOnlyList<Vector2Int> positions, bool isKnown)
        {
            foreach (var position in positions)
            {
                SetKnown(position, isKnown);
                SetVisibility(position, VisibilityAt(position));
            }
        }

        private void ApplyTile(TileSpec tile)
        {
            _tileSets[tile.Position] = tile.Set;
            SetKnown(tile.Position, tile.IsKnown);
            DrawTile(tile);
        }

        private void SetKnown(Vector2Int position, bool isKnown)
        {
            if (isKnown)
                _knownPositions.Add(position);
            else
                _knownPositions.Remove(position);
        }

        private TileVisibility VisibilityAt(Vector2Int position)
        {
            if (!_knownPositions.Contains(position))
                return TileVisibility.Transparent;
            return _sight.Contains(position) ? TileVisibility.Visible : TileVisibility.Translucent;
        }

        private void DrawTile(TileSpec tile)
        {
            var position = tile.Position;
            var index = tile.Kind switch
            {
                TileKind.Floor => _shopRect.HasValue && _shopRect.Value.Contains(position) ? 1 : 0,
                TileKind.Water => 2,
                TileKind.Wall => 3,
                TileKind.UnbreakableWall => 3,
                _ => throw new ArgumentOutOfRangeException(nameof(tile), tile.Kind, null)
            };
            var (palette, underTile) = _tilePalette.GetTile(tile.Set, index);
            _tileView.SetTile(position, palette, underTile);

            switch (tile.Kind)
            {
                case TileKind.Floor:
                    _minimapController.SetFloor(position);
                    break;
                case TileKind.Water:
                    _minimapController.SetWater(position);
                    break;
                case TileKind.Wall:
                    _minimapController.SetWall(position);
                    break;
                case TileKind.UnbreakableWall:
                    _minimapController.SetUnbreakableWall(position);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tile), tile.Kind, null);
            }
        }

        private void SetVisibility(Vector2Int position, TileVisibility visibility)
        {
            _tileView.SetTileVisibility(position, visibility);
            _overlayTileView.SetTileVisibility(position, visibility);
            _minimapController.SetTileVisibility(position, visibility);
        }

        private void ApplyOverlay(OverlaySpec overlay)
        {
            var position = overlay.Position;
            switch (overlay.Kind)
            {
                case OverlayKind.Grass:
                    _overlayTileView.SetGrass(position, _tileSets.TryGetValue(position, out var set) ? set : _defaultTileSet);
                    break;
                case OverlayKind.FloatingIce:
                    _overlayTileView.SetIce(position);
                    break;
                case null:
                    _overlayTileView.RemoveTile(position);
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(overlay), overlay.Kind, null);
            }

            _overlayTileView.SetTileVisibility(position, VisibilityAt(position));
        }
    }
}
