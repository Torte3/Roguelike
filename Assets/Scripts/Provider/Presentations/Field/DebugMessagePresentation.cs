#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Field
{
    internal sealed class DebugMessagePresentation : Presentation<DebugMessage>
    {
        protected override IEnumerable<ViewOp> OpsOf(DebugMessage worldEvent)
        {
            yield return Logs.Line(worldEvent.Message);
        }
    }
}
