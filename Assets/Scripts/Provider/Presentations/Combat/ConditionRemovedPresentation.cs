#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.WorldEvents;
using View.Playback;

namespace Provider.Presentations.Combat
{
    internal sealed class ConditionRemovedPresentation : ConditionChangedPresentation<ConditionRemoved>
    {
        protected override Guid? SubjectOf(ConditionRemoved worldEvent) => KeyOf(worldEvent.Entity);

        protected override IEnumerable<ViewCommand> CommandsOf(ConditionRemoved worldEvent) =>
            ParticlesOf(worldEvent.Entity, worldEvent.Particles);

        protected override IEnumerable<ViewCommand> VisibleCommandsOf(ConditionRemoved worldEvent) =>
            worldEvent.IsLast ? LogOf(worldEvent.Label, worldEvent.ConditionLog) : Enumerable.Empty<ViewCommand>();
    }
}
