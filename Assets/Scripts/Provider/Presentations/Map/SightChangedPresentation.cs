#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Map
{
    internal sealed class SightChangedPresentation : Presentation<SightChanged>
    {
        protected override IEnumerable<ViewOp> OpsOf(SightChanged worldEvent)
        {
            yield return new ChangeSight(worldEvent.VisibleArea);
        }
    }
}
