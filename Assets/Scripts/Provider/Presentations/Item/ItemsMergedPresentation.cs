#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Item
{
    internal sealed class ItemsMergedPresentation : Presentation<ItemsMerged>
    {
        protected override IEnumerable<ViewOp> OpsOf(ItemsMerged worldEvent)
        {
            foreach (var op in ShopPrices.Of(worldEvent.Shop))
                yield return op;
            foreach (var op in InventoryRows.Updated(worldEvent.Inventory))
                yield return op;
            yield return Logs.Line(
                $"{Names.Of(worldEvent.Label)}は{Names.Of(worldEvent.BaseItemName)}と{Names.Of(worldEvent.MergedItemName)}を合成した。");
        }
    }
}
