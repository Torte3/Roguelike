#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Item;
using Domain.Model.Memento;

namespace Domain.Service.Items
{
    public static class ItemExtension
    {
        public static TResult Match<TResult>(this IItem item,
            Func<Item, TResult> itemFunc,
            Func<DirectWeapon, TResult> directWeaponFunc,
            Func<RangedWeapon, TResult> rangedWeaponFunc)
        {
            return item switch
            {
                Item uniqueItem => itemFunc(uniqueItem),
                DirectWeapon directWeapon => directWeaponFunc(directWeapon),
                RangedWeapon rangedWeapon => rangedWeaponFunc(rangedWeapon),
                _ => throw new ArgumentException("Invalid item type")
            };
        }

        public static async UniTask<TResult> Match<TResult>(this IItem item,
            Func<Item, UniTask<TResult>> itemFunc,
            Func<DirectWeapon, UniTask<TResult>> directWeaponFunc,
            Func<RangedWeapon, UniTask<TResult>> rangedWeaponFunc)
        {
            return item switch
            {
                Item uniqueItem => await itemFunc(uniqueItem),
                DirectWeapon directWeapon => await directWeaponFunc(directWeapon),
                RangedWeapon rangedWeapon => await rangedWeaponFunc(rangedWeapon),
                _ => throw new ArgumentException("Invalid item type")
            };
        }

        public static TResult Match<TResult>(this IItemMemento memento,
            Func<ItemMemento, TResult> itemFunc,
            Func<DirectWeaponMemento, TResult> directWeaponFunc,
            Func<RangedWeaponMemento, TResult> rangedWeaponFunc)
        {
            return memento switch
            {
                ItemMemento itemMemento => itemFunc(itemMemento),
                DirectWeaponMemento directWeaponMemento => directWeaponFunc(directWeaponMemento),
                RangedWeaponMemento rangedWeaponMemento => rangedWeaponFunc(rangedWeaponMemento),
                _ => throw new ArgumentException("Invalid item type")
            };
        }

        public static TResult Match<TResult>(this IItemData data,
            Func<ItemData, TResult> itemFunc,
            Func<DirectWeaponData, TResult> directWeaponFunc,
            Func<RangedWeaponData, TResult> rangedWeaponFunc)
        {
            return data switch
            {
                ItemData itemData => itemFunc(itemData),
                DirectWeaponData directWeaponData => directWeaponFunc(directWeaponData),
                RangedWeaponData rangedWeaponData => rangedWeaponFunc(rangedWeaponData),
                _ => throw new ArgumentException("Invalid item data type")
            };
        }

        public static IItemMemento Serialize(this IItem item)
        {
            return item.Match<IItemMemento>(
                item => item.Serialize(),
                directWeapon => directWeapon.Serialize(),
                rangedWeapon => rangedWeapon.Serialize()
            );
        }

        public static IItem Deserialize(this IItemMemento memento)
        {
            return memento.Match<IItem>(
                itemMemento => new Item(itemMemento),
                directWeaponMemento => new DirectWeapon(directWeaponMemento),
                rangedWeaponMemento => new RangedWeapon(rangedWeaponMemento)
            );
        }

        public static IItem Clone(this IItem item)
        {
            return item.Serialize().Deserialize();
        }

        public static IItemMemento Build(this IItemData data, int upgradeCount = 0, bool isCursed = false, ItemState state = ItemState.None, EnemyData? mimic = null)
        {
            return data.Match<IItemMemento>(
                itemData => Item.Build(
                    itemData,
                    upgradeCount: upgradeCount,
                    isCursed: isCursed,
                    state: state,
                    mimic: mimic),
                directWeaponData => DirectWeapon.Build(
                    directWeaponData,
                    upgradeCount: upgradeCount,
                    isCursed: isCursed,
                    state: state,
                    mimic: mimic),
                rangedWeaponData => RangedWeapon.Build(
                    rangedWeaponData,
                    upgradeCount: upgradeCount,
                    isCursed: isCursed,
                    state: state,
                    mimic: mimic)
            );
        }
    }
}