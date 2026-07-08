#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Presentations.Inventory;
using View.Playback;

namespace Provider.Presentations.Item
{
    internal sealed class ItemUpdatedPresentation : Presentation<ItemUpdated>
    {
        private readonly InventoryRows _inventoryRows;

        public ItemUpdatedPresentation(InventoryRows inventoryRows)
        {
            _inventoryRows = inventoryRows;
        }

        protected override IEnumerable<ViewCommand> CommandsOf(ItemUpdated worldEvent)
        {
            return _inventoryRows.ItemUpdated(worldEvent.Item);
        }
    }
}
