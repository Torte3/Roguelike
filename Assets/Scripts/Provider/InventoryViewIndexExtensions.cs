#nullable enable
using Domain.Model.Item;
using Unity.Logging;
using View.UI;

namespace Provider
{
    public static class InventoryViewIndexExtensions
    {
        public static InventoryViewIndex ToInventoryViewIndex(this ItemFocus focus)
        {
            Log.Verbose($"ToInventoryViewIndex: focus: {focus}");
            if (focus.IsOnGroundItem)
                return InventoryViewIndex.GroundItem;
            else if (focus.IsOnEmpty)
                return InventoryViewIndex.Empty;
            else
                return new(focus.Index);
        }
        public static ItemFocus ToItemFocus(this InventoryViewIndex index)
        {
            Log.Verbose($"ToItemFocus: index: {index}");
            if (index.IsOnGroundItem)
                return ItemFocus.GroundItem;
            else if (index.IsOnEmpty)
                return ItemFocus.Empty;
            else
                return new(index.Index);
        }
    }
}