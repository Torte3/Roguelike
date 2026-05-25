#nullable enable
using Domain.Model.Item;

namespace Domain.Model.Character
{
    public enum InventorySortingMode
    {
        None,
        ByCategory,
        ByPrice,
    }
    public interface IInventory : IStorage
    {
        public bool CanAddOrNot(IItem? item);
        public void AddOrNot(IItem? item);
        public bool CanReplaceOrRemove(IItem? item, int index);
        public IItem ReplaceOrRemove(IItem? item, int index);
        public void Sort(InventorySortingMode sortingMode, ItemMarketPriceTable market);
    }
}