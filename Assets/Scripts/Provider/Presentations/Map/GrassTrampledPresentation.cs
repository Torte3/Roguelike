#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Map
{
    internal sealed class GrassTrampledPresentation : Presentation<GrassTrampled>
    {
        protected override IEnumerable<ViewOp> OpsOf(GrassTrampled worldEvent)
        {
            yield return new PlaySe(SeKind.GrassWalk);
        }
    }
}
