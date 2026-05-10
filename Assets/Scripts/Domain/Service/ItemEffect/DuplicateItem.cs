using System;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Memento;
using Domain.Service.Items;
using Utilities;

namespace Domain.Service.ItemEffect
{
    [Serializable]
    public class DuplicateItem : IItemEffect
    {
        public bool CanApplyTo(IPlayer player, IItem item)
        {
            return player.Character.Inventory.Contains(item)
                   && player.Character.Inventory.CanAddToEmpty();
        }

        public void Apply(IPlayer player, IItem item, IEntity itemHolder, ItemPlaceholders itemPlaceholders)
        {
            var duplicatedItem = Duplicate(item);
            player.Character.Inventory.AddToEmpty(duplicatedItem);
        }

        public float EvaluatePrice()
        {
            return 1000;
        }

        public string Info()
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
                artifactMemento => artifactMemento.CopyWith(
                    baseItem: artifactMemento.BaseItem.CopyWith(id: newId)
                )
            );
            return copiedMemento.Deserialize();
        }
    }
}
