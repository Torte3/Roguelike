#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Combat
{
    internal sealed class StatueStruckPresentation : EntityPresentation<StatueStruck>
    {
        protected override bool WaitsForMovers(StatueStruck worldEvent)
        {
            return worldEvent.IsVisible;
        }

        protected override IEnumerable<ViewOp> OpsOf(StatueStruck worldEvent)
        {
            yield return new ShakeEntity(worldEvent.Entity.Key());
        }
    }
}
