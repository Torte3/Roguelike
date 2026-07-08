using System;
using Domain.Model.Character;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Item.Slots;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.WorldEvents;
using Domain.Service.Items;
using Utilities;

namespace Domain.Service.ItemEffect
{
    [Serializable]
    public class DuplicateItem : IItemEffect
    {
        public bool CanApplyTo(IItem item, IActor player)
        {
            return player.Inventory.Contains(item)
                   && player.Inventory.CanAddToEmpty();
        }

        public void Apply(IItemSlot slot, IEntity itemHolder, IActor player, IMap map)
        {
            var duplicatedItem = Duplicate(slot.Item);
            player.Inventory.AddToEmpty(duplicatedItem);
            map.RecordUnderfootChanges();
            map.Events.Record(new ItemChanged(itemHolder.Entity.IsVisible, slot.Item.NameIn(map), ItemChangeKind.Duplicated,
                duplicatedItem.LookIn(map)));
        }

        public float EvaluatePrice()
        {
            return 1000;
        }

        public string Description()
        {
            return "複製";
        }

        private static IItem Duplicate(IItem item)
        {
            var newId = Id<IItem>.Generate();
            var copiedMemento = item.Serialize().Match<IItemMemento>(
                itemMemento => itemMemento.CopyWith(
                    baseItem: itemMemento.BaseItem.CopyWith(id: newId)
                ),
                directWeaponMemento => directWeaponMemento.CopyWith(
                    baseItem: directWeaponMemento.BaseItem.CopyWith(id: newId)
                ),
                rangedWeaponMemento => rangedWeaponMemento.CopyWith(
                    baseItem: rangedWeaponMemento.BaseItem.CopyWith(id: newId)
                ),
                equipmentItemMemento => equipmentItemMemento.CopyWith(
                    baseItem: equipmentItemMemento.BaseItem.CopyWith(id: newId)
                )
            );
            return copiedMemento.Deserialize();
        }
    }
}
