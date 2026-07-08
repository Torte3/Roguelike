#nullable enable
using View.UI.Inventory;

namespace View.Playback.RowChanges
{
    public sealed record RowRemoved(int Index) : RowChange(Index)
    {
        internal override void ApplyTo(InventoryView inventory)
        {
            inventory.Remove(Index);
        }
    }
}
