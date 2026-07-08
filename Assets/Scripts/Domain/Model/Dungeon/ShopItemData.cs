using System;
using Domain.Model.Item.Data;
using Domain.Model.Item.Rarities;

namespace Domain.Model.Dungeon
{
    [Serializable]
    public class ShopItemData
    {
        public RarityWeightTable<ItemData> Items;
    }
}