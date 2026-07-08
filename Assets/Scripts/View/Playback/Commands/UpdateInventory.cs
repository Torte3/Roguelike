#nullable enable
using System.Collections.Generic;
using View.Playback.RowChanges;

namespace View.Playback.Commands
{
    public sealed record UpdateInventory(IReadOnlyList<RowChange> Changes, int Count, int Capacity) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            foreach (var change in Changes)
                change.ApplyTo(context.Inventory);
            context.Status.SetInventory(Count, Capacity);
        }
    }
}
