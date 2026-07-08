#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Map
{
    internal sealed class OverlayTilesChangedPresentation : Presentation<OverlayTilesChanged>
    {
        protected override IEnumerable<ViewOp> OpsOf(OverlayTilesChanged worldEvent)
        {
            yield return new ChangeOverlays(worldEvent.Tiles.Select(tile => Tiles.Of(tile.Position, tile.Category)).ToList());
        }
    }
}
