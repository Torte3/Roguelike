#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Effect.Skill;
using Domain.Model.Item;
using Domain.Model.WorldEvents;
using Provider.Presentations.Translation;
using Provider.Texts;
using View.Playback;
using View.Playback.Commands;
using Utilities;
using View.Playback.RowChanges;
using View.UI.Inventory;

namespace Provider.Presentations.Inventory
{
    public sealed class InventoryRows
    {
        private const string BareHandHeading = "アイテムを使わずに攻撃";

        private readonly ShopPrices _prices;
        private readonly InventoryIcons _icons;
        private readonly List<ItemLook> _items = new();
        private Underfoot _underfoot = new NothingUnderfoot();
        private Layout _layout;

        public InventoryRows(ShopPrices prices, InventoryIcons icons)
        {
            _prices = prices;
            _icons = icons;
            _layout = LayoutOf(0, false, "");
        }

        internal ViewCommand Rebuilt(InventoryContents inventory, IReadOnlySkill bareHand)
        {
            _layout = LayoutOf(inventory.Capacity, inventory.CanRemoveItems, bareHand.Description());
            _underfoot = new NothingUnderfoot();
            _items.Clear();
            _items.AddRange(inventory.Items);
            return new ShowInventory(_items.Select(InventoryRowOf).ToList(), _layout.EmptyUnderfoot, _layout.BareHand,
                _layout.Capacity);
        }

        internal ViewCommand Changed(IReadOnlyList<InventoryRowChange> changes)
        {
            var rows = changes.Select(RowOf).ToList();
            foreach (var change in changes)
                Apply(change);
            return Updated(rows);
        }

        private RowChange RowOf(InventoryRowChange change)
        {
            return change switch
            {
                InventoryRowInserted inserted => new RowInserted(inserted.Index, InventoryRowOf(inserted.Item)),
                InventoryRowRemoved removed => new RowRemoved(removed.Index),
                _ => throw new ArgumentOutOfRangeException(nameof(change), change, null),
            };
        }

        private void Apply(InventoryRowChange change)
        {
            switch (change)
            {
                case InventoryRowInserted inserted:
                    _items.Insert(inserted.Index, inserted.Item);
                    break;
                case InventoryRowRemoved removed:
                    _items.RemoveAt(removed.Index);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(change), change, null);
            }
        }

        internal IEnumerable<ViewCommand> Repainted()
        {
            return new[]
            {
                Updated(_items.Select((item, index) => (RowChange)new RowReplaced(index, InventoryRowOf(item))).ToList()),
                UnderfootShown(),
            };
        }

        internal IEnumerable<ViewCommand> Repainted(Id<IItem> item)
        {
            var changes = _items
                .Select((look, index) => (Look: look, Index: index))
                .Where(row => row.Look.Id == item)
                .Select(row => (RowChange)new RowReplaced(row.Index, InventoryRowOf(row.Look)))
                .ToList();
            if (changes.Count > 0)
                yield return Updated(changes);
            yield return UnderfootShown();
        }

        internal IEnumerable<ViewCommand> ItemUpdated(ItemLook item)
        {
            var commands = new List<ViewCommand>();
            var index = _items.FindIndex(look => look.Id == item.Id);
            if (index >= 0)
            {
                _items[index] = item;
                commands.Add(Updated(new RowChange[] { new RowReplaced(index, InventoryRowOf(item)) }));
            }

            if (_underfoot is ItemUnderfoot underfoot && underfoot.Item.Id == item.Id)
                commands.Add(UnderfootChanged(new ItemUnderfoot(item)));
            return commands;
        }

        internal ViewCommand UnderfootChanged(Underfoot underfoot)
        {
            _underfoot = underfoot;
            return UnderfootShown();
        }

        private ViewCommand Updated(IReadOnlyList<RowChange> changes) => new UpdateInventory(changes, _items.Count, _layout.Capacity);

        private ViewCommand UnderfootShown()
        {
            return new ShowUnderfoot(UnderfootRowOf(_underfoot));
        }

        private IItemRow UnderfootRowOf(Underfoot underfoot)
        {
            return underfoot switch
            {
                ItemUnderfoot item => new UnderfootRow(RowOf(item.Item, true)),
                NothingUnderfoot => _layout.EmptyUnderfoot,
                _ => throw new ArgumentOutOfRangeException(nameof(underfoot), underfoot, null),
            };
        }

        private Layout LayoutOf(int capacity, bool canRemoveItems, string bareHandDescription)
        {
            var info = $"{BareHandHeading}\n\n{bareHandDescription}";
            return new Layout(capacity, canRemoveItems, new EmptyRow(UnderfootRow.Heading, _icons.Underfoot, info),
                new EmptyRow("", _icons.BareHand, info));
        }

        private IItemRow InventoryRowOf(ItemLook item) => RowOf(item, _layout.CanRemoveItems);

        private IItemRow RowOf(ItemLook item, bool canSelect)
        {
            return new ItemRow(item, new ItemNameText(item.Name).Text + _prices.SuffixOf(item.Id), new ItemDescriptionText(item.Description).Text,
                canSelect);
        }

        private sealed record Layout(int Capacity, bool CanRemoveItems, IItemRow EmptyUnderfoot, IItemRow BareHand);
    }
}
