#nullable enable
using Domain.Model.WorldEvents;
using Provider.Presentations.Inventory;
using Provider.Presentations.Translation;

namespace Provider.Presentations.Field
{
    internal sealed class ShopExitedPresentation : ShopRelationPresentation<ShopExited>
    {
        public ShopExitedPresentation(BGMSelector bgm, ShopPrices shopPrices, InventoryRows inventoryRows)
            : base(bgm, shopPrices, inventoryRows, new OutsideShop())
        {
        }
    }
}
