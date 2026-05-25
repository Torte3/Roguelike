#nullable enable
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Map;

namespace Domain.Model.Item
{
    public record ItemFocus
    {
        public int Index { get; init; }
        public bool IsInInventory => !IsOnGroundItem && !IsOnEmpty;
        public bool IsOnGroundItem { get; init; }
        public bool IsOnEmpty { get; init; }
        public ItemFocus(int index, bool isGroundItem, bool isEmpty)
        {
            Index = index;
            IsOnGroundItem = isGroundItem;
            IsOnEmpty = isEmpty;
        }
        public ItemFocus(int index) : this(index, false, false) { }
        public static readonly ItemFocus GroundItem = new(-1, true, false);
        public static readonly ItemFocus Empty = new(-1, false, true);
        public IItem? GetItem(IInventory inventory, IMap map)
        {
            if (IsOnEmpty)
                return null;
            if (IsOnGroundItem)
                return map.Items.At(map.Player.Character.Entity.CurrentPosition).FirstOrDefault()?.Item;
            return inventory.GetItem(Index);
        }
        public bool IsOnItem(IInventory inventory, IMap map) => GetItem(inventory, map) != null;
        public bool IsOnItem(IInventory inventory, IMap map, out IItem item)
        {
            item = GetItem(inventory, map);
            return item != null;
        }
    }
}