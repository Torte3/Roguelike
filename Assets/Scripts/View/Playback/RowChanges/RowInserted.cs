#nullable enable
using View.UI.Inventory;

namespace View.Playback.RowChanges
{
    public sealed record RowInserted(int Index, IItemRow Data) : RowChange(Index)
    {
        internal override void ApplyTo(InventoryView inventory)
        {
            inventory.Insert(Index, Data);
        }
    }
}
