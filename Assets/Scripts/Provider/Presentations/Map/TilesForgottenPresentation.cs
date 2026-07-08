#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Presentations.Tile;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Map
{
    internal sealed class TilesForgottenPresentation : Presentation<TilesForgotten>
    {
        private readonly TileVisibilities _tileVisibilities;

        public TilesForgottenPresentation(TileVisibilities tileVisibilities)
        {
            _tileVisibilities = tileVisibilities;
        }

        protected override IEnumerable<ViewCommand> CommandsOf(TilesForgotten worldEvent)
        {
            yield return new SetTileVisibilities(_tileVisibilities.Forgotten(worldEvent.Positions));
        }
    }
}
