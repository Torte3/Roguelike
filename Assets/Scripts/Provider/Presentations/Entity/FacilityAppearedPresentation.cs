#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Entity
{
    internal sealed class FacilityAppearedPresentation : EntityPresentation<FacilityAppeared>
    {
        protected override IEnumerable<ViewOp> OpsOf(FacilityAppeared worldEvent)
        {
            var key = worldEvent.Entity.Key();
            var appearance = worldEvent.Appearance;
            yield return new AddEntity(key, Prefabs.NameOf(appearance), worldEvent.Entity.Position,
                appearance.Icon, appearance.IsShiny, worldEvent.Entity.IsVisible);
            yield return new SetUsable(key, worldEvent.IsUsable);
            yield return new SetInteractable(key, worldEvent.IsUsable);
        }
    }
}
