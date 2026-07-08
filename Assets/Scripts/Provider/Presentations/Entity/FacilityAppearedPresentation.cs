#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Presentations.Translation;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Entity
{
    internal sealed class FacilityAppearedPresentation : Presentation<FacilityAppeared>
    {
        protected override Guid? SubjectOf(FacilityAppeared worldEvent) => KeyOf(worldEvent.Entity);

        protected override IEnumerable<ViewCommand> CommandsOf(FacilityAppeared worldEvent)
        {
            var key = KeyOf(worldEvent.Entity);
            yield return new AppearanceFigure(worldEvent.Appearance).AddCommandFor(worldEvent.Entity);
            yield return new SetUsable(key, worldEvent.IsUsable);
            yield return new SetInteractable(key, worldEvent.IsUsable);
        }
    }
}
