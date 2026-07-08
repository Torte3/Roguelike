#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Combat
{
    internal sealed class ProjectileFlewPresentation : Presentation<ProjectileFlew>
    {
        protected override bool WaitsForMovers(ProjectileFlew worldEvent) => true;

        protected override IEnumerable<ViewOp> OpsOf(ProjectileFlew worldEvent)
        {
            yield return Flights.Fly(worldEvent.Flight);
        }
    }
}
