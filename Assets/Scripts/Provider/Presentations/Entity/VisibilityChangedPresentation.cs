#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Entity
{
    internal sealed class VisibilityChangedPresentation : Presentation<VisibilityChanged>
    {
        protected override Guid? SubjectOf(VisibilityChanged worldEvent) => KeyOf(worldEvent.Entity);

        protected override IEnumerable<ViewCommand> CommandsOf(VisibilityChanged worldEvent)
        {
            yield return new SetEntityVisibility(KeyOf(worldEvent.Entity), worldEvent.Entity.IsVisible);
        }
    }
}
