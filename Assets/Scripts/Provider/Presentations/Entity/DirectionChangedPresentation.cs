#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Entity
{
    internal sealed class DirectionChangedPresentation : EntityPresentation<DirectionChanged>
    {
        protected override IEnumerable<ViewOp> OpsOf(DirectionChanged worldEvent)
        {
            yield return new TurnCharacter(worldEvent.Entity.Key(), worldEvent.Direction);
        }
    }
}
