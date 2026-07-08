#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Commands;
using View.Playback.Values;

namespace Provider.Presentations.Map
{
    internal sealed class GrassTrampledPresentation : Presentation<GrassTrampled>
    {
        protected override IEnumerable<ViewCommand> CommandsOf(GrassTrampled worldEvent)
        {
            yield return new PlaySE(SEKind.GrassWalk);
        }
    }
}
