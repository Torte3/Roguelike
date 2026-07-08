#nullable enable

namespace View.Playback.Ops
{
    public sealed record HideShopPrices : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.ShopInfo.SetVisibility(false);
        }
    }
}
