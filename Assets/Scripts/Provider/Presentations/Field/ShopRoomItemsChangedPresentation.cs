#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;

namespace Provider.Presentations.Field
{
    internal sealed class ShopRoomItemsChangedPresentation : Presentation<ShopRoomItemsChanged>
    {
        protected override IEnumerable<ViewOp> OpsOf(ShopRoomItemsChanged worldEvent)
        {
            return ShopPrices.Of(worldEvent.Shop);
        }
    }
}
