#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Map
{
    internal sealed class TilesChangedPresentation : Presentation<TilesChanged>
    {
        protected override IEnumerable<ViewOp> OpsOf(TilesChanged worldEvent)
        {
            yield return new ChangeTiles(worldEvent.Tiles.Select(Tiles.Of).ToList());
        }
    }
}
