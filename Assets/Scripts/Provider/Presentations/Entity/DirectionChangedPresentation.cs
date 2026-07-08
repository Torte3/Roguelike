#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Entity
{
    internal sealed class DirectionChangedPresentation : Presentation<DirectionChanged>
    {
        protected override Guid? SubjectOf(DirectionChanged worldEvent) => KeyOf(worldEvent.Entity);

        protected override IEnumerable<ViewCommand> CommandsOf(DirectionChanged worldEvent)
        {
            yield return new TurnCharacter(KeyOf(worldEvent.Entity), worldEvent.Direction);
        }
    }
}
