#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.WorldEvents;
using View.Playback;

namespace Provider.Presentations.Combat
{
    internal sealed class ConditionInflictedPresentation : ConditionChangedPresentation<ConditionInflicted>
    {
        protected override Guid? SubjectOf(ConditionInflicted worldEvent) => KeyOf(worldEvent.Entity);

        protected override IEnumerable<ViewCommand> CommandsOf(ConditionInflicted worldEvent) =>
            ParticlesOf(worldEvent.Entity, worldEvent.Particles);

        protected override IEnumerable<ViewCommand> VisibleCommandsOf(ConditionInflicted worldEvent) =>
            worldEvent.IsNew ? LogOf(worldEvent.Label, worldEvent.ConditionLog) : Enumerable.Empty<ViewCommand>();
    }
}
