using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Utilities.Table

{
    [Serializable]
    public class Table<T> : ITable<T>
    {
        [RequiredListLength(1, null)]
        [SerializeField]
        private List<WeightedItem> items = new();

        public int Count => items.Count;

        public IReadOnlyList<T> GetItems() => items.Select(item => item.Item).ToList();

        public Table()
        {
        }

        public Table(IReadOnlyList<T> source)
        {
            items = source.Select(item => new WeightedItem { Item = item, Weight = 1f }).ToList();
        }

        public T GetRandomItem() => GetRandomItems(1)[0];

        public List<T> GetRandomItems(int count) =>
            items.GetWeightedAtRandom(count, w => w.Weight).Select(w => w.Item).ToList();

        [Serializable]
        public class WeightedItem
        {
            [HorizontalGroup("Group 1", 0.7f)]
            [HideLabel]
            public T Item;

            [HorizontalGroup("Group 1", 0.3f)]
            [HideLabel]
            public float Weight = 1;
        }
    }
}