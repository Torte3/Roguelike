#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Model.WorldEvents;

namespace Domain.Model.Item
{
    public static class ItemLookExtensions
    {
        public static ItemLook LookIn(this IReadOnlyItem item, IReadOnlyMap map)
        {
            var player = map.PlayerCharacter;
            return new ItemLook(
                item.Id,
                item.NameIn(map),
                item.Describe(player, map.ItemPlaceholders),
                item.Icon,
                item.CanAttemptUseOrThrow,
                item.IsEquipped.IsNone && item.HasActivatableSkill ? item.RemainingUses.CurrentValue : null,
                item.IsEquipped.UnwrapOr(false),
                item.IsCursed,
                item.IsShiny,
                player.IsKnownItem(item),
                item.IsCurseIdentified,
                map.Shop is { IsInside: { CurrentValue: true } } shop
                    ? new ShopPrice(shop.GetPrice(item, map), item.State == ItemState.ShopItem)
                    : null);
        }

        public static InventoryLook InventoryLookIn(this IActor holder, IMap map, params IReadOnlyItem[] changedItems)
        {
            var changes = holder.Inventory.TakeRowChanges();
            var items = holder.Inventory.AllItems.ToList();
            var indexes = InsertedIndexes(changes)
                .Concat(changedItems.Select(changed => items.FindIndex(item => ReferenceEquals(item, changed))));
            return LookIn(holder, map, changes, items, indexes);
        }

        public static InventoryLook WholeInventoryLookIn(this IActor holder, IMap map)
        {
            var changes = holder.Inventory.TakeRowChanges();
            var items = holder.Inventory.AllItems.ToList();
            return LookIn(holder, map, changes, items, Enumerable.Range(0, items.Count));
        }

        private static InventoryLook LookIn(IActor holder, IMap map, IReadOnlyList<InventoryRowChange> changes,
            IReadOnlyList<IReadOnlyItem> items, IEnumerable<int> indexes)
        {
            return new InventoryLook(
                holder.Entity.Id,
                holder.AppearsInteractable(map),
                holder.Label.IsPlayer
                    ? new InventoryContents(
                        changes,
                        indexes.Where(index => index >= 0).Distinct().OrderBy(index => index)
                            .Select(index => new InventoryRow(index, items[index].LookIn(map))).ToList(),
                        items.Count,
                        holder.Inventory.Capacity.CurrentValue,
                        holder.Inventory.CanRemoveItem)
                    : null);
        }

        private static IEnumerable<int> InsertedIndexes(IReadOnlyList<InventoryRowChange> changes)
        {
            var indexes = new List<int>();
            foreach (var change in changes)
            {
                if (change.Kind == InventoryRowChangeKind.Inserted)
                {
                    indexes = indexes.Select(index => index >= change.Index ? index + 1 : index).Append(change.Index).ToList();
                }
                else
                {
                    indexes = indexes.Where(index => index != change.Index)
                        .Select(index => index > change.Index ? index - 1 : index).ToList();
                }
            }

            return indexes;
        }

        public static ShopLook? ShopLookIn(this IMap map)
        {
            return map.Shop is { IsInside: { CurrentValue: true } } shop ? shop.LookIn(map) : null;
        }

        public static ShopLook LookIn(this IReadOnlyShop shop, IMap map)
        {
            return new ShopLook(shop.GetPurchasePrice(map), shop.GetSalePrice(map), shop.ClerkId, shop.ClerkAppearsInteractable(map));
        }

        public static Underfoot PlayerUnderfoot(this IReadOnlyMap map)
        {
            return new Underfoot(map.ItemAt(map.PlayerCharacter.Position)?.LookIn(map));
        }
    }
}
