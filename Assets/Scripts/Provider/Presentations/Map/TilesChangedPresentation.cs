#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Presentations.Tile;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Map
{
    internal sealed class TilesChangedPresentation : Presentation<TilesChanged>
    {
        private readonly TileKindClassifier _tileKinds;
        private readonly TileVisibilities _tileVisibilities;

        public TilesChangedPresentation(TileKindClassifier tileKinds, TileVisibilities tileVisibilities)
        {
            _tileKinds = tileKinds;
            _tileVisibilities = tileVisibilities;
        }

        protected override IEnumerable<ViewCommand> CommandsOf(TilesChanged worldEvent)
        {
            yield return new ChangeTiles(_tileKinds.TilesChanged(worldEvent.Tiles), _tileVisibilities.TilesChanged(worldEvent.Tiles));
        }
    }
}
