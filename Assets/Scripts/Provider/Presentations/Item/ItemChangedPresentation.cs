#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Item
{
    internal sealed class ItemChangedPresentation : Presentation<ItemChanged>
    {
        protected override IEnumerable<ViewOp> OpsOf(ItemChanged worldEvent)
        {
            foreach (var op in ShopPrices.Of(worldEvent.Shop))
                yield return op;
            foreach (var op in InventoryRows.Updated(worldEvent.Inventory))
                yield return op;
            foreach (var op in InventoryRows.Of(worldEvent.Underfoot))
                yield return op;
            if (worldEvent.IsVisible && LogOf(worldEvent) is { } log)
                yield return Logs.Line(log);
        }

        private static string? LogOf(ItemChanged worldEvent)
        {
            var item = Names.Of(worldEvent.ItemName);
            return worldEvent.Kind switch
            {
                ItemChangeKind.Cursed => $"{item}は呪われた",
                ItemChangeKind.Uncursed => $"{item}の呪いは解かれた",
                ItemChangeKind.CurseRevealed => $"{item}は呪われていた",
                ItemChangeKind.BecameUndiscardable => $"{item}は捨てられなくなった",
                ItemChangeKind.Repaired => $"{item}は修理された",
                ItemChangeKind.NotConsumed => $"{item}は消費しなかった",
                ItemChangeKind.Upgraded => $"{item}は強化された",
                ItemChangeKind.Downgraded => $"{item}は強化が解除された",
                ItemChangeKind.Burned => $"{item}は灰になった",
                ItemChangeKind.Consumed or ItemChangeKind.Used or ItemChangeKind.Equipped or ItemChangeKind.Unequipped
                    or ItemChangeKind.CurseIdentified or ItemChangeKind.UsedWhileCursed or ItemChangeKind.Recharged
                    or ItemChangeKind.Transformed or ItemChangeKind.Duplicated => null,
                _ => throw new ArgumentOutOfRangeException(nameof(worldEvent), worldEvent.Kind, null),
            };
        }
    }
}
