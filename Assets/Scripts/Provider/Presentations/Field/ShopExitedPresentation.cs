#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Field
{
    internal sealed class ShopExitedPresentation : Presentation<ShopExited>
    {
        protected override IEnumerable<ViewOp> OpsOf(ShopExited worldEvent)
        {
            foreach (var op in ShopPrices.Hidden(worldEvent.Shop))
                yield return op;
            foreach (var op in InventoryRows.Updated(worldEvent.Inventory))
                yield return op;
            foreach (var op in InventoryRows.Of(worldEvent.Underfoot))
                yield return op;
            yield return new PlayBgm(BgmTrack.Normal);
        }
    }
}
