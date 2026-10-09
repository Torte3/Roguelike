#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Entity
{
    internal sealed class EntityAppearedPresentation : EntityPresentation<EntityAppeared>
    {
        protected override IEnumerable<ViewOp> OpsOf(EntityAppeared worldEvent)
        {
            foreach (var op in InventoryRows.Of(worldEvent.Underfoot))
                yield return op;
            var appearance = worldEvent.Appearance;
            yield return new AddEntity(worldEvent.Entity.Key(), Prefabs.NameOf(appearance), worldEvent.Entity.Position,
                appearance.Icon, appearance.IsShiny, worldEvent.Entity.IsVisible);
        }
    }
}
