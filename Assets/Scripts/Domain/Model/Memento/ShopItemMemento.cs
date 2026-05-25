using System;
using Domain.Model.Item;
using UnityEngine;
using Utilities;

namespace Domain.Model.Memento
{
    [Serializable]
    public class ShopItemMemento
    {
        [SerializeField] private string _id;
        public Id<IItem> Id => new Id<IItem>(_id);
        [field: SerializeField] public int Price { get; private set; }

        public ShopItemMemento(Id<IItem> id, int price)
        {
            _id = id.ToString();
            Price = price;
        }
    }
}