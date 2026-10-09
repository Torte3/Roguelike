#nullable enable
using View.UI;

namespace View.Playback.Ops
{
    public sealed record ShowUnderfoot(ItemViewData? Row) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Inventory.UpdateGroundItem(Row);
        }
    }
}
