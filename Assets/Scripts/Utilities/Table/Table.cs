using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

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

        public T GetRandomItem()
        {
            return items[items.Select(items => items.Weight).WeightedIndex()].Item;
        }

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