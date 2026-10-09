#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Item
{
    internal sealed class InventoryRefreshedForDebugPresentation : Presentation<InventoryRefreshedForDebug>
    {
        protected override IEnumerable<ViewOp> OpsOf(InventoryRefreshedForDebug worldEvent)
        {
            foreach (var op in InventoryRows.Rebuilt(worldEvent.Inventory, false))
                yield return op;
            foreach (var op in InventoryRows.Of(worldEvent.Underfoot))
                yield return op;
        }
    }
}
