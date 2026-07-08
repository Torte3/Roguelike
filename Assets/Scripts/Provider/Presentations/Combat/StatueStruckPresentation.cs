#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Combat
{
    internal sealed class StatueStruckPresentation : Presentation<StatueStruck>
    {
        protected override Guid? SubjectOf(StatueStruck worldEvent) => KeyOf(worldEvent.Entity);

        protected override bool IsWaitingForMovers(StatueStruck worldEvent)
        {
            return worldEvent.IsVisible;
        }

        protected override IEnumerable<ViewCommand> CommandsOf(StatueStruck worldEvent)
        {
            yield return new ShakeEntity(KeyOf(worldEvent.Entity));
        }
    }
}
