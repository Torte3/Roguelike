#nullable enable
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Map;

namespace Domain.Model.Item.Slots
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
        public IItemSlot? GetSlot(IInventory inventory, IMap map)
        {
            if (IsOnEmpty)
                return null;
            if (IsOnGroundItem)
            {
                var groundItem = map.Items.At(map.Player.Character.Entity.CurrentPosition).FirstOrDefault();
                return groundItem == null ? null : new GroundItemSlot(groundItem, map);
            }
            var item = inventory.GetItem(Index);
            return item == null ? null : new InventoryItemSlot(inventory, item);
        }
        public IItem? GetItem(IInventory inventory, IMap map) => GetSlot(inventory, map)?.Item;
        public bool IsOnItem(IInventory inventory, IMap map) => GetItem(inventory, map) != null;
        public bool IsOnItem(IInventory inventory, IMap map, out IItem item)
        {
            item = GetItem(inventory, map);
            return item != null;
        }
    }
}