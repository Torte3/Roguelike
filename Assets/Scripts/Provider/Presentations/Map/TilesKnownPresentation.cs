#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Presentations.Tile;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Map
{
    internal sealed class TilesKnownPresentation : Presentation<TilesKnown>
    {
        private readonly TileVisibilities _tileVisibilities;

        public TilesKnownPresentation(TileVisibilities tileVisibilities)
        {
            _tileVisibilities = tileVisibilities;
        }

        protected override IEnumerable<ViewCommand> CommandsOf(TilesKnown worldEvent)
        {
            yield return new SetTileVisibilities(_tileVisibilities.Known(worldEvent.Positions));
        }
    }
}
