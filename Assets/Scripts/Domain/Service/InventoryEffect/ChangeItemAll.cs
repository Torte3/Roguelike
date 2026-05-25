using System;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Service.Items;
using UnityEngine;
using Utilities.Serialize;

namespace Domain.Service.InventoryEffect
{
    [Serializable]
    public class ChangeItemAll : IInventoryEffect
    {
        [SerializeField] private ScriptableObjectSerializable<ItemData> _item;

        public void Apply(IPlayer player, IStorage storage, IEntity itemHolder, ItemPlaceholders itemPlaceholders)
        {
            for (var i = 0; i < storage.Capacity.CurrentValue; i++)
            {
                if (storage.HasItemAt(i))
                    storage.Replace(new Item(_item.Value), i);
            }
        }

        public float EvaluatePrice()
        {
            return 100 * 5;
        }

        public string Info()
        {
            return $"変化({_item.Value.name})";
        }
    }
}