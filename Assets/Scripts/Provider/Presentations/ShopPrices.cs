#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations
{
    internal static class ShopPrices
    {
        public static IEnumerable<ViewOp> Of(ShopLook? shop)
        {
            if (shop == null)
                yield break;

            yield return new ShowShopPrices(shop.PurchasePrice, shop.SalePrice);
            yield return ClerkOf(shop);
        }

        public static IEnumerable<ViewOp> Hidden(ShopLook shop)
        {
            yield return new HideShopPrices();
            yield return ClerkOf(shop);
        }

        private static ViewOp ClerkOf(ShopLook shop) => new SetInteractable(shop.Clerk.Key(), shop.ClerkAppearsInteractable);
    }
}
