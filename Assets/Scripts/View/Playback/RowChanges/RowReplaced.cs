#nullable enable
using View.UI.Inventory;

namespace View.Playback.RowChanges
{
    public sealed record RowReplaced(int Index, IItemRow Data) : RowChange(Index)
    {
        internal override void ApplyTo(InventoryView inventory)
        {
            inventory.Replace(Index, Data);
        }
    }
}
