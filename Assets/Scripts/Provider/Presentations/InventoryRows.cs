#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.WorldEvents;
using Provider.Texts;
using View.Playback;
using View.Playback.Ops;
using View.UI;

namespace Provider.Presentations
{
    internal static class InventoryRows
    {
        public static IEnumerable<ViewOp> Updated(InventoryLook? inventory)
        {
            if (inventory == null)
                yield break;

            yield return new SetInteractable(inventory.Holder.Key(), inventory.HolderAppearsInteractable);
            if (inventory.Contents is { } contents)
                yield return new UpdateInventory(
                    contents.Changes.Select(ChangeOf).ToList(),
                    contents.Rows.Select(row => new InventoryRowView(row.Index, RowOf(row.Item, contents.CanRemoveItems))).ToList(),
                    contents.Count,
                    contents.Capacity);
        }

        public static IEnumerable<ViewOp> Rebuilt(InventoryLook? inventory, bool resetsFocus)
        {
            if (inventory == null)
                yield break;

            yield return new SetInteractable(inventory.Holder.Key(), inventory.HolderAppearsInteractable);
            if (inventory.Contents is { } contents)
                yield return new ShowInventory(
                    contents.Rows.Select(row => RowOf(row.Item, contents.CanRemoveItems)).ToList(),
                    contents.Capacity,
                    resetsFocus);
        }

        private static RowChange ChangeOf(InventoryRowChange change)
        {
            return change.Kind switch
            {
                InventoryRowChangeKind.Inserted => new RowInserted(change.Index),
                InventoryRowChangeKind.Removed => new RowRemoved(change.Index),
                _ => throw new System.ArgumentOutOfRangeException(nameof(change), change.Kind, null),
            };
        }

        public static IEnumerable<ViewOp> Of(Underfoot? underfoot)
        {
            if (underfoot != null)
                yield return new ShowUnderfoot(underfoot.Item == null ? null : RowOf(underfoot.Item, true));
        }

        private static ItemViewData RowOf(ItemLook item, bool canSelect)
        {
            return new ItemViewData(
                Names.Of(item.Name) + PriceSuffix(item.Price),
                item.CanAttemptUseOrThrow,
                item.Icon,
                canSelect,
                item.Count,
                item.IsEquipped,
                item.IsCursed,
                item.IsShiny,
                item.IsKnown,
                item.IsCurseIdentified,
                ItemDescriptionText.Of(item.Description));
        }

        private static string PriceSuffix(ShopPrice? price)
        {
            return price == null
                ? ""
                : $"\n{price.Price}G".Paint(price.IsShopItem ? Tint.ShopItemPrice : Tint.OwnItemPrice);
        }
    }
}
