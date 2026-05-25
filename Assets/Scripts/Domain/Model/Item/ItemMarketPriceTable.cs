#nullable enable
using System;
using Domain.Model.Dungeon;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model.Item
{
    [CreateAssetMenu(fileName = "ItemMarketPriceTable", menuName = "ScriptableObject/ItemMarketPriceTable")]
    public class ItemMarketPriceTable : ScriptableObject
    {
        [Title("Category base price")]
        [MinValue(0)] public float Potions = 60f;
        [MinValue(0)] public float Scrolls = 90f;
        [MinValue(0)] public float Books = 180f;
        [MinValue(0)] public float Wands = 200f;
        [MinValue(0)] public float Weapons = 160f;
        [MinValue(0)] public float Artifacts = 220f;

        [Title("Rarity multiplier")]
        [MinValue(0)] public float Common = 1.0f;
        [MinValue(0)] public float Uncommon = 1.4f;
        [MinValue(0)] public float Rare = 2.0f;
        [MinValue(0)] public float Epic = 2.8f;
        [MinValue(0)] public float Legendary = 4.0f;

        public float GetCategoryBase(ItemCategory category)
        {
            return category switch
            {
                ItemCategory.Potions => Potions,
                ItemCategory.Scrolls => Scrolls,
                ItemCategory.Books => Books,
                ItemCategory.Wands => Wands,
                ItemCategory.Weapons => Weapons,
                ItemCategory.Artifacts => Artifacts,
                _ => 0f
            };
        }

        public float GetRarityMultiplier(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common => Common,
                Rarity.Uncommon => Uncommon,
                Rarity.Rare => Rare,
                Rarity.Epic => Epic,
                Rarity.Legendary => Legendary,
                _ => 1.0f
            };
        }

        public float GetBasePrice(ItemCategory category, Rarity rarity)
        {
            return Mathf.Max(1f, GetCategoryBase(category) * GetRarityMultiplier(rarity));
        }
    }
}

