#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Map;
using Domain.Model.WorldEvents;
using Provider.Presentations.Tile;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Map
{
    internal sealed class OverlayTilesChangedPresentation : Presentation<OverlayTilesChanged>
    {
        private readonly TileVisibilities _tileVisibilities;

        public OverlayTilesChangedPresentation(TileVisibilities tileVisibilities)
        {
            _tileVisibilities = tileVisibilities;
        }

        protected override IEnumerable<ViewCommand> CommandsOf(OverlayTilesChanged worldEvent)
        {
            var placed = worldEvent.Tiles
                .Where(tile => tile.Category.HasValue)
                .Select(tile => (tile.Position, tile.Category.Value));
            yield return new ChangeOverlays(_tileVisibilities.OverlaysOf(placed));
            yield return new RemoveOverlays(worldEvent.Tiles
                .Where(tile => !tile.Category.HasValue)
                .Select(tile => tile.Position)
                .ToList());
        }
    }
}
