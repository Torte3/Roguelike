using System;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.WorldEvents;
using Domain.Service.Items;
using UnityEngine;
using Utilities.Serialize;

namespace Domain.Service.InventoryEffect
{
    [Serializable]
    public class ChangeItemAll : IInventoryEffect
    {
        [SerializeField] private ScriptableObjectSerializable<ItemData> _item;

        public void Apply(IStorage storage, IEntity itemHolder, IMap map)
        {
            for (var i = 0; i < storage.Capacity.CurrentValue; i++)
            {
                if (!storage.HasItemAt(i, out var item))
                    continue;
                var name = item.NameIn(map);
                var changed = new Item(_item.Value);
                storage.Replace(changed, i);
                map.Events.Record(new ItemChanged(itemHolder.Entity.IsVisible, name, ItemChangeKind.Transformed, changed.LookIn(map),
                    itemHolder.HeldItemsIn(map), itemHolder.UnderfootIn(map), map.ShopLookIn()));
            }
        }

        public float EvaluatePrice()
        {
            return 100 * 5;
        }

        public string Description()
        {
            return $"変化({_item.Value.name})";
        }
    }
}