#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Item
{
    internal sealed class ItemRenamedPresentation : EntityPresentation<ItemRenamed>
    {
        protected override IEnumerable<ViewOp> OpsOf(ItemRenamed worldEvent)
        {
            foreach (var op in InventoryRows.Updated(worldEvent.Inventory))
                yield return op;
            foreach (var op in InventoryRows.Of(worldEvent.Underfoot))
                yield return op;
        }
    }
}
