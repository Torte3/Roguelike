#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Map
{
    internal sealed class ReturnedToTitlePresentation : Presentation<ReturnedToTitle>
    {
        protected override IEnumerable<ViewCommand> CommandsOf(ReturnedToTitle worldEvent)
        {
            yield return new ClearLog();
        }
    }
}
