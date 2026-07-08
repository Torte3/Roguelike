#nullable enable

namespace View.Playback.Commands
{
    public sealed record ResetInventoryFocus : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Inventory.ResetFocus();
        }
    }
}
