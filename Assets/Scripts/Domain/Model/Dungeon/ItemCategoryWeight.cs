using System;
using Utilities;

namespace Domain.Model.Dungeon
{
    [Serializable]
    public class ItemCategoryWeight
    {
        public float Potions;
        public float Scrolls;
        public float Books;
        public float Wands;
        public float Weapons;
        public float Artifacts;
        public float Others;

        internal ItemCategory GetRandomCategory()
        {
            return new[] { Potions, Scrolls, Books, Wands, Weapons, Artifacts, Others }.WeightedIndex() switch
            {
                0 => ItemCategory.Potions,
                1 => ItemCategory.Scrolls,
                2 => ItemCategory.Books,
                3 => ItemCategory.Wands,
                4 => ItemCategory.Weapons,
                5 => ItemCategory.Artifacts,
                6 => ItemCategory.Others,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}