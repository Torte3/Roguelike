#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Entity
{
    internal sealed class FacilityExhaustedPresentation : EntityPresentation<FacilityExhausted>
    {
        protected override IEnumerable<ViewOp> OpsOf(FacilityExhausted worldEvent)
        {
            var key = worldEvent.Entity.Key();
            yield return new SetUsable(key, false);
            yield return new SetInteractable(key, false);
            if (worldEvent.Icon != null)
                yield return new SetIcon(key, worldEvent.Icon);
        }
    }
}
