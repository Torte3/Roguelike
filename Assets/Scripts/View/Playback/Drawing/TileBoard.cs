#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using View.Map;
using View.Playback.Values;

namespace View.Playback.Drawing
{
    public sealed class TileBoard
    {
        private readonly TileViewController _tileView;
        private readonly OverlayTileViewController _overlayTileView;
        private readonly MinimapController _minimapController;
        private readonly TilePalette _tilePalette;

        [Inject]
        public TileBoard(TileViewController tileView, OverlayTileViewController overlayTileView,
            MinimapController minimapController, TilePalette tilePalette)
        {
            _tileView = tileView;
            _overlayTileView = overlayTileView;
            _minimapController = minimapController;
            _tilePalette = tilePalette;
        }

        internal void Enter(IReadOnlyList<TileAppearance> tiles, IReadOnlyList<OverlayAppearance> overlays,
            IReadOnlyList<PositionedTileVisibility> visibilities)
        {
            _tileView.Clear();
            _overlayTileView.Clear();
            _minimapController.Clear();

            foreach (var tile in tiles)
                DrawTile(tile);

            foreach (var overlay in overlays)
                ApplyOverlay(overlay);

            SetVisibilities(visibilities);
        }

        internal void Change(IReadOnlyList<TileAppearance> tiles, IReadOnlyList<PositionedTileVisibility> visibilities)
        {
            foreach (var tile in tiles)
                DrawTile(tile);

            SetVisibilities(visibilities);
        }

        internal void ChangeOverlays(IReadOnlyList<OverlayAppearance> overlays)
        {
            foreach (var overlay in overlays)
                ApplyOverlay(overlay);
        }

        internal void RemoveOverlays(IReadOnlyList<Vector2Int> positions)
        {
            foreach (var position in positions)
                _overlayTileView.RemoveTile(position);
        }

        internal void SetVisibilities(IReadOnlyList<PositionedTileVisibility> tiles)
        {
            foreach (var tile in tiles)
                SetVisibility(tile.Position, tile.Visibility);
        }

        private void DrawTile(TileAppearance tile)
        {
            var (palette, underTile) = _tilePalette.GetTile(tile.Set, tile.PaletteIndex);
            _tileView.SetTile(tile.Position, palette, underTile);
            tile.Minimap.Draw(_minimapController, tile.Position);
        }

        private void SetVisibility(Vector2Int position, TileVisibility visibility)
        {
            _tileView.SetTileVisibility(position, visibility);
            _overlayTileView.SetTileVisibility(position, visibility);
            _minimapController.SetTileVisibility(position, visibility);
        }

        private void ApplyOverlay(OverlayAppearance overlay)
        {
            var position = overlay.Position;
            switch (overlay.Kind)
            {
                case OverlayKind.Grass:
                    _overlayTileView.SetGrass(position, overlay.Set);
                    break;
                case OverlayKind.FloatingIce:
                    _overlayTileView.SetIce(position);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(overlay.Kind), overlay.Kind, null);
            }

            _overlayTileView.SetTileVisibility(position, overlay.Visibility);
        }
    }
}
