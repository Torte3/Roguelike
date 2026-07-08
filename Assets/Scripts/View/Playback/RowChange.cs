#nullable enable
using View.UI;

namespace View.Playback
{
    public abstract record RowChange(int Index)
    {
        internal abstract void ApplyTo(InventoryView inventory);
    }
}
