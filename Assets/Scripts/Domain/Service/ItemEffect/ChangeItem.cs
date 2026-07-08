using System;
using Domain.Model.Character;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Item.Data;
using Domain.Model.Item.Slots;
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

        public bool CanApplyTo(IItem item, IActor player)
        {
            return true;
        }

        public void Apply(IItemSlot slot, IEntity itemHolder, IActor player, IMap map)
        {
            var name = slot.Item.NameIn(map);
            var changed = new Item(_item.Value);
            slot.Replace(changed);
            map.RecordUnderfootChanges();
            map.Events.Record(new ItemChanged(itemHolder.Entity.IsVisible, name, ItemChangeKind.Transformed, changed.LookIn(map)));
            map.RecordShopChanges();
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