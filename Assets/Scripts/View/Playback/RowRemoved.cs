#nullable enable
using View.UI;

namespace View.Playback
{
    public sealed record RowRemoved(int Index) : RowChange(Index)
    {
        internal override void ApplyTo(InventoryView inventory)
        {
            inventory.Remove(Index);
        }
    }
}
