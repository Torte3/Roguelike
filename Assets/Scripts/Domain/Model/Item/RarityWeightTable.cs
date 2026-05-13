using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;
using Utilities.Table;

namespace Domain.Model.Item
{
    [Serializable]
    public class RarityWeightTable<T> : ICorrectionTable<T> where T : IHasRarity
    {
        [RequiredListLength(1, null)]
        [SerializeField]
        private List<T> items = new();

        public List<T> Items => items;

        public RarityWeightTable(List<T> items)
        {
            this.items = items;
        }

        public T GetRandomItem(float progress)
        {
            var itemsByRarity = items
                .Where(item => item != null)
                .GroupBy(item => item.Rarity)
                .Where(group => group.Any())
                .ToList();

            if (!itemsByRarity.Any())
            {
                throw new InvalidOperationException("Item is not found");
            }

            var rarityIndex = itemsByRarity.Select(group => group.Key.GetWeight(progress)).WeightedIndex();
            var selectedRarity = itemsByRarity[rarityIndex].Key;

            return itemsByRarity[rarityIndex].GetAtRandom();
        }
    }
}