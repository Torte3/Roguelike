#nullable enable
namespace View.UI
{
    public record InventoryViewIndex
    {
        public int Index { get; init; }
        public bool IsOnGroundItem { get; init; }
        public bool IsOnEmpty { get; init; }
        private InventoryViewIndex(int index, bool isGroundItem, bool isEmpty)
        {
            Index = index;
            IsOnGroundItem = isGroundItem;
            IsOnEmpty = isEmpty;
        }
        public InventoryViewIndex(int index)
        {
            Index = index;
            IsOnGroundItem = false;
            IsOnEmpty = false;
        }
        public static readonly InventoryViewIndex GroundItem = new(-1, true, false);
        public static readonly InventoryViewIndex Empty = new(-1, false, true);
        public override string ToString()
        {
            if (IsOnGroundItem)
                return $"GroundItem";
            else if (IsOnEmpty)
                return $"Empty";
            else if (!IsOnGroundItem && !IsOnEmpty)
            {
                return $"Index: {Index}";
            }
            else
                return $"Error";
        }
    }
}