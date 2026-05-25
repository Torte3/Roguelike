using System;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Service.Items;
using UnityEngine;
using Utilities.Serialize;

namespace Domain.Service.ItemEffect
{
    [Serializable]
    public class ChangeItem : IItemEffect
    {
        [SerializeField] private ScriptableObjectSerializable<ItemData> _item;

        public bool CanApplyTo(IPlayer player, IItem item)
        {
            return true;
        }

        public void Apply(IPlayer player, IItem item, IEntity itemHolder, ItemPlaceholders itemPlaceholders)
        {
            player.Character.Inventory.Replace(new Item(_item.Value), player.Character.Inventory.GetItemIndex(item).Value);
        }

        public float EvaluatePrice()
        {
            return 100;
        }

        public string Info()
        {
            return $"変化({_item.Value.name})";
        }
    }
}