#nullable enable

namespace View.Playback.Commands
{
    public sealed record HideShopPrices : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.ShopInfo.SetVisibility(false);
        }
    }
}
