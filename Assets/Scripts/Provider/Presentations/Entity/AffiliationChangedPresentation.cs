#nullable enable
using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Entity
{
    internal sealed class AffiliationChangedPresentation : EntityPresentation<AffiliationChanged>
    {
        protected override IEnumerable<ViewOp> OpsOf(AffiliationChanged worldEvent)
        {
            var key = worldEvent.Entity.Key();
            yield return new SetAffiliationMarker(key,
                worldEvent.Affiliation == AffiliationType.Enemy, worldEvent.Affiliation == AffiliationType.Ally);
            yield return new SetInteractable(key, worldEvent.AppearsInteractable);
        }
    }
}
