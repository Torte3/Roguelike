#nullable enable
using System.Collections.Generic;

namespace View.Playback.Ops
{
    public sealed record UpdateInventory(IReadOnlyList<RowChange> Changes, IReadOnlyList<InventoryRowView> Rows, int Count,
        int Capacity) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            foreach (var change in Changes)
                change.ApplyTo(context.Inventory);
            foreach (var row in Rows)
                context.Inventory.Replace(row.Index, row.Data);
            context.Status.SetInventory(Count, Capacity);
        }
    }
}
