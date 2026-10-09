#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Item
{
    internal sealed class ItemGivenPresentation : EntityPresentation<ItemGiven>
    {
        protected override IEnumerable<ViewOp> OpsOf(ItemGiven worldEvent)
        {
            foreach (var op in ShopPrices.Of(worldEvent.Shop))
                yield return op;
            foreach (var op in InventoryRows.Updated(worldEvent.Inventory))
                yield return op;
            foreach (var op in InventoryRows.Updated(worldEvent.GiverInventory))
                yield return op;
            if (worldEvent.IsVisible)
                yield return Logs.Line($"{Names.Of(worldEvent.Label)}に{Names.Of(worldEvent.ItemName)}を渡した。");
        }
    }
}
