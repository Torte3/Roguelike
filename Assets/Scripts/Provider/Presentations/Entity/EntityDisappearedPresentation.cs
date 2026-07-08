#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Entity
{
    internal sealed class EntityDisappearedPresentation : EntityPresentation<EntityDisappeared>
    {
        protected override IEnumerable<ViewOp> OpsOf(EntityDisappeared worldEvent)
        {
            foreach (var op in InventoryRows.Of(worldEvent.Underfoot))
                yield return op;
            yield return new RemoveEntity(worldEvent.Entity.Key());
        }
    }
}
