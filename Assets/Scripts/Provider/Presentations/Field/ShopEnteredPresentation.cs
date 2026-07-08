#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Field
{
    internal sealed class ShopEnteredPresentation : Presentation<ShopEntered>
    {
        protected override IEnumerable<ViewOp> OpsOf(ShopEntered worldEvent)
        {
            foreach (var op in ShopPrices.Of(worldEvent.Shop))
                yield return op;
            foreach (var op in InventoryRows.Updated(worldEvent.Inventory))
                yield return op;
            foreach (var op in InventoryRows.Of(worldEvent.Underfoot))
                yield return op;
            yield return new PlayBgm(BgmTrack.Shop);
        }
    }
}
