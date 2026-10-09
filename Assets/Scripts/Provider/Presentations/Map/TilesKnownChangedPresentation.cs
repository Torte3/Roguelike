#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Map
{
    internal sealed class TilesKnownChangedPresentation : Presentation<TilesKnownChanged>
    {
        protected override IEnumerable<ViewOp> OpsOf(TilesKnownChanged worldEvent)
        {
            yield return new SetTilesKnown(worldEvent.Positions, worldEvent.IsKnown);
        }
    }
}
