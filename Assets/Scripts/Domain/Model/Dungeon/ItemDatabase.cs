using Domain.Model.Item;
using Utilities.Table;

namespace Domain.Model.Dungeon
{
    public class ItemDatabase : ICorrectionTable<IItemData>
    {
        private MasterItemDataBase _masterItemDataBase;
        private ItemCategoryWeight _itemCategoryWeight;

        public ItemDatabase(MasterItemDataBase masterItemDataBase, ItemCategoryWeight itemCategoryWeight)
        {
            _masterItemDataBase = masterItemDataBase;
            _itemCategoryWeight = itemCategoryWeight;
        }

        public IItemData GetRandomItem(float progress)
        {
            return _masterItemDataBase.GetRandomItem(_itemCategoryWeight.GetRandomCategory(), progress);
        }

        public IItemData GetRandomItem(ItemCategory category, float progress)
        {
            return _masterItemDataBase.GetRandomItem(category, progress);
        }
    }
}