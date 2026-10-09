#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Entity
{
    internal sealed class MaxHpChangedPresentation : EntityPresentation<MaxHpChanged>
    {
        protected override IEnumerable<ViewOp> OpsOf(MaxHpChanged worldEvent)
        {
            return HealthOps.Of(worldEvent);
        }
    }
}
