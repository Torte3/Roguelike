#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Presentations.Inventory;
using Provider.Presentations.Translation;
using View.Playback;

namespace Provider.Presentations.Field
{
    internal sealed class ItemPriceChangedPresentation : Presentation<ItemPriceChanged>
    {
        private readonly InventoryRows _inventoryRows;
        private readonly ShopPrices _shopPrices;

        public ItemPriceChangedPresentation(InventoryRows inventoryRows, ShopPrices shopPrices)
        {
            _inventoryRows = inventoryRows;
            _shopPrices = shopPrices;
        }

        protected override IEnumerable<ViewCommand> CommandsOf(ItemPriceChanged worldEvent)
        {
            _shopPrices.PriceChanged(worldEvent);
            return _inventoryRows.Repainted(worldEvent.Item);
        }
    }
}
