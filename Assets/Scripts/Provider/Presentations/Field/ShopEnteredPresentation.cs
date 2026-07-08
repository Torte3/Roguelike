#nullable enable
using Domain.Model.WorldEvents;
using Provider.Presentations.Inventory;
using Provider.Presentations.Translation;

namespace Provider.Presentations.Field
{
    internal sealed class ShopEnteredPresentation : ShopRelationPresentation<ShopEntered>
    {
        public ShopEnteredPresentation(BGMSelector bgm, ShopPrices shopPrices, InventoryRows inventoryRows)
            : base(bgm, shopPrices, inventoryRows, new InsideShop())
        {
        }
    }
}
