#nullable enable
using View.UI.Inventory;

namespace View.Playback.Commands
{
    public sealed record ShowUnderfoot(IItemRow Row) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Inventory.UpdateGroundItem(Row);
        }
    }
}
