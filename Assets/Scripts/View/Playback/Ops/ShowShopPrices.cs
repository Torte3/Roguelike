#nullable enable

namespace View.Playback.Ops
{
    public sealed record ShowShopPrices(int PurchasePrice, int SalePrice) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.ShopInfo.SetVisibility(true);
            context.ShopInfo.SetInfo(PurchasePrice, SalePrice);
        }
    }
}
