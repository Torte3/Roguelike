#nullable enable
using Domain.Model.WorldEvents;
using Provider.Presentations.Inventory;
using Provider.Texts;
using System;
using System.Collections.Generic;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Item
{
    internal sealed class ItemChangedPresentation : Presentation<ItemChanged>
    {
        private readonly InventoryRows _inventoryRows;

        public ItemChangedPresentation(InventoryRows inventoryRows)
        {
            _inventoryRows = inventoryRows;
        }

        protected override IEnumerable<ViewCommand> CommandsOf(ItemChanged worldEvent)
        {
            return _inventoryRows.ItemUpdated(worldEvent.Item);
        }

        protected override IEnumerable<ViewCommand> VisibleCommandsOf(ItemChanged worldEvent)
        {
            if (LogOf(worldEvent) is { } log)
                yield return AddLog.Line(log);
        }

        private string? LogOf(ItemChanged worldEvent)
        {
            var itemName = new ItemNameText(worldEvent.NameBefore).Text;
            return worldEvent.Kind switch
            {
                ItemChangeKind.Cursed => $"{itemName}は呪われた",
                ItemChangeKind.Uncursed => $"{itemName}の呪いは解かれた",
                ItemChangeKind.BecameUndiscardable => $"{itemName}は捨てられなくなった",
                ItemChangeKind.Repaired => $"{itemName}は修理された",
                ItemChangeKind.NotConsumed => $"{itemName}は消費しなかった",
                ItemChangeKind.Upgraded => $"{itemName}は強化された",
                ItemChangeKind.Downgraded => $"{itemName}は強化が解除された",
                ItemChangeKind.Burned => $"{itemName}は灰になった",
                ItemChangeKind.Consumed or ItemChangeKind.Used or ItemChangeKind.Equipped or ItemChangeKind.Unequipped
                    or ItemChangeKind.UsedWhileCursed or ItemChangeKind.Recharged
                    or ItemChangeKind.Transformed or ItemChangeKind.Duplicated => null,
                _ => throw new ArgumentOutOfRangeException(nameof(worldEvent.Kind), worldEvent.Kind, null),
            };
        }
    }
}
