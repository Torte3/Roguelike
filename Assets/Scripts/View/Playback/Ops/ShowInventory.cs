#nullable enable
using System.Collections.Generic;
using System.Linq;
using View.UI;

namespace View.Playback.Ops
{
    public sealed record ShowInventory(IReadOnlyList<ItemViewData> Rows, int Capacity, bool ResetsFocus) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Inventory.Reset(Rows.ToList(), ResetsFocus);
            context.Status.SetInventory(Rows.Count, Capacity);
        }
    }
}
