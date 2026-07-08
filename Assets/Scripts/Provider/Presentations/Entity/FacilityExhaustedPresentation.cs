#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Entity
{
    internal sealed class FacilityExhaustedPresentation : Presentation<FacilityExhausted>
    {
        protected override Guid? SubjectOf(FacilityExhausted worldEvent) => KeyOf(worldEvent.Entity);

        protected override IEnumerable<ViewCommand> CommandsOf(FacilityExhausted worldEvent)
        {
            var key = KeyOf(worldEvent.Entity);
            yield return new SetUsable(key, false);
            yield return new SetInteractable(key, false);
            if (worldEvent.Icon != null)
                yield return new SetIcon(key, worldEvent.Icon);
        }
    }
}
