#nullable enable
using System.Collections.Generic;
using System.Linq;
using View.UI.Inventory;

namespace View.Playback.Commands
{
    public sealed record ShowInventory(IReadOnlyList<IItemRow> Rows, IItemRow UnderfootRow, IItemRow BareHandRow,
        int Capacity) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Inventory.Reset(Rows.ToList(), UnderfootRow, BareHandRow);
            context.Status.SetInventory(Rows.Count, Capacity);
        }
    }
}
