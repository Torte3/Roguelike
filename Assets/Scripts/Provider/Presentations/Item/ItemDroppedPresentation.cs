#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Item
{
    internal sealed class ItemDroppedPresentation : EntityPresentation<ItemDropped>
    {
        protected override IEnumerable<ViewOp> OpsOf(ItemDropped worldEvent)
        {
            foreach (var op in InventoryRows.Updated(worldEvent.Inventory))
                yield return op;
            if (worldEvent.IsVisible)
                yield return Logs.Line($"{Names.Of(worldEvent.Label)}は{Names.Of(worldEvent.ItemName)}を落とした");
        }
    }
}
