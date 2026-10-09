using System;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.WorldEvents;
using Domain.Service.Items;
using UnityEngine;
using Utilities.Serialize;

namespace Domain.Service.ItemEffect
{
    [Serializable]
    public class ChangeItem : IItemEffect
    {
        [SerializeField] private ScriptableObjectSerializable<ItemData> _item;

        public bool CanApplyTo(IItem item, IMap map)
        {
            return true;
        }

        public void Apply(IItem item, IEntity itemHolder, IMap map)
        {
            var name = item.NameIn(map);
            var changed = new Item(_item.Value);
            map.Player.Character.Inventory.Replace(changed, map.Player.Character.Inventory.GetItemIndex(item).Value);
            map.Events.Record(new ItemChanged(itemHolder.Entity.IsVisible, name, ItemChangeKind.Transformed, changed.LookIn(map),
                itemHolder.HeldItemsIn(map), itemHolder.UnderfootIn(map), map.ShopLookIn()));
        }

        public float EvaluatePrice()
        {
            return 100;
        }

        public string Description()
        {
            return $"変化({_item.Value.name})";
        }
    }
}