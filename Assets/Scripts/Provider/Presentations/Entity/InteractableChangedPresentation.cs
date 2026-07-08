#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Entity
{
    internal sealed class InteractableChangedPresentation : Presentation<InteractableChanged>
    {
        protected override Guid? SubjectOf(InteractableChanged worldEvent) => KeyOf(worldEvent.Entity);

        protected override IEnumerable<ViewCommand> CommandsOf(InteractableChanged worldEvent)
        {
            yield return new SetInteractable(KeyOf(worldEvent.Entity), worldEvent.AppearsInteractable);
        }
    }
}
