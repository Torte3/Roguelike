#nullable enable

namespace View.Playback.Commands
{
    public sealed record ShowShopPrices(int PurchasePrice, int SalePrice) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.ShopInfo.SetVisibility(true);
            context.ShopInfo.SetInfo(PurchasePrice, SalePrice);
        }
    }
}
