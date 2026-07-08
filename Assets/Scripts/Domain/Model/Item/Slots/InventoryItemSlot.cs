#nullable enable
using Domain.Model.Character;

namespace Domain.Model.Item.Slots
{
    public sealed class InventoryItemSlot : IItemSlot
    {
        private readonly IInventory _inventory;

        public InventoryItemSlot(IInventory inventory, IItem item)
        {
            _inventory = inventory;
            Item = item;
        }

        public IItem Item { get; }

        public void Replace(IItem item)
        {
            _inventory.Replace(item, _inventory.GetItemIndex(Item)!.Value);
        }
    }
}
