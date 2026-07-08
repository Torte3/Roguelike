#nullable enable
using View.UI.Inventory;

namespace View.Playback.RowChanges
{
    public abstract record RowChange(int Index)
    {
        internal abstract void ApplyTo(InventoryView inventory);
    }
}
