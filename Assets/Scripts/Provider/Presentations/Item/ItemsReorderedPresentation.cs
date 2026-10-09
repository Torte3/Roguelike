#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Item
{
    internal sealed class ItemsReorderedPresentation : EntityPresentation<ItemsReordered>
    {
        protected override IEnumerable<ViewOp> OpsOf(ItemsReordered worldEvent)
        {
            foreach (var op in InventoryRows.Updated(worldEvent.Inventory))
                yield return op;
        }
    }
}
