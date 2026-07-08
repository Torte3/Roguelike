#nullable enable
using View.UI;

namespace View.Playback
{
    public sealed record RowInserted(int Index) : RowChange(Index)
    {
        internal override void ApplyTo(InventoryView inventory)
        {
            inventory.Insert(Index, inventory.EmptyRow);
        }
    }
}
