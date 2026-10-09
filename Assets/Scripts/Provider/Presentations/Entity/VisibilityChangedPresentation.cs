#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Entity
{
    internal sealed class VisibilityChangedPresentation : EntityPresentation<VisibilityChanged>
    {
        protected override IEnumerable<ViewOp> OpsOf(VisibilityChanged worldEvent)
        {
            yield return new SetEntityVisibility(worldEvent.Entity.Key(), worldEvent.Entity.IsVisible);
        }
    }
}
