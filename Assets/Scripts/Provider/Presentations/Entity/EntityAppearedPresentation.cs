#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Presentations.Translation;
using View.Playback;

namespace Provider.Presentations.Entity
{
    internal sealed class EntityAppearedPresentation : Presentation<EntityAppeared>
    {
        protected override Guid? SubjectOf(EntityAppeared worldEvent) => KeyOf(worldEvent.Entity);

        protected override IEnumerable<ViewCommand> CommandsOf(EntityAppeared worldEvent)
        {
            yield return new AppearanceFigure(worldEvent.Appearance).AddCommandFor(worldEvent.Entity);
        }
    }
}
