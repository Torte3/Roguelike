#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Texts;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Field
{
    internal sealed class ShopSettledPresentation : Presentation<ShopSettled>
    {
        protected override IEnumerable<ViewOp> OpsOf(ShopSettled worldEvent)
        {
            foreach (var op in ShopPrices.Of(worldEvent.Shop))
                yield return op;
            foreach (var op in InventoryRows.Updated(worldEvent.Inventory))
                yield return op;
            foreach (var op in InventoryRows.Of(worldEvent.Underfoot))
                yield return op;
            var name = Names.Of(worldEvent.Label);
            if (worldEvent.Received > 0)
                yield return Logs.Line($"{name}は{$"{worldEvent.Received}G".Paint(Tint.Gain)}受け取った");
            if (worldEvent.Paid > 0)
                yield return Logs.Line($"{name}は{$"{worldEvent.Paid}G".Paint(Tint.Notice)}支払った");
            yield return new PlaySe(SeKind.ShopCheckout);
            yield return new SetStatusMoney(worldEvent.Money);
        }
    }
}
