#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.Item;
using Domain.Model.WorldEvents;
using Provider.Texts;
using Utilities;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Translation
{
    public sealed class ShopPrices
    {
        private static readonly IPriceDisplay Shown = new ShownPrices();
        private static readonly IPriceDisplay Hidden = new HiddenPrices();

        private readonly Dictionary<Id<IItem>, ItemPrice> _prices = new();
        private IPriceDisplay _display = Hidden;
        private Totals _totals = new(0, 0);

        internal ViewCommand RelationChanged(ShopRelation relation)
        {
            _display = DisplayOf(relation);
            return _display.TotalsOf(_totals);
        }

        private static IPriceDisplay DisplayOf(ShopRelation relation)
        {
            return relation switch
            {
                NoShop => Hidden,
                OutsideShop => Hidden,
                InsideShop => Shown,
                ShopAfterTheft => Hidden,
                _ => throw new ArgumentOutOfRangeException(nameof(relation), relation, null),
            };
        }

        internal ViewCommand TotalsChanged(ShopTotalsChanged totals)
        {
            _totals = new Totals(totals.PurchasePrice, totals.SalePrice);
            return _display.TotalsOf(_totals);
        }

        internal void PriceChanged(ItemPriceChanged price)
        {
            _prices[price.Item] = new ItemPrice(price.Price, price.IsShopItem);
        }

        internal string SuffixOf(Id<IItem> item)
        {
            return _prices.TryGetValue(item, out var price) ? _display.SuffixOf(price) : "";
        }

        private interface IPriceDisplay
        {
            public ViewCommand TotalsOf(Totals totals);
            public string SuffixOf(ItemPrice price);
        }

        private sealed record Totals(int PurchasePrice, int SalePrice);

        private sealed record ItemPrice(int Price, bool IsShopItem);

        private sealed class ShownPrices : IPriceDisplay
        {
            public ViewCommand TotalsOf(Totals totals) => new ShowShopPrices(totals.PurchasePrice, totals.SalePrice);

            public string SuffixOf(ItemPrice price)
            {
                return (price.IsShopItem ? Tint.ShopItemPrice : Tint.OwnItemPrice).Paint($"\n{price.Price}G");
            }
        }

        private sealed class HiddenPrices : IPriceDisplay
        {
            public ViewCommand TotalsOf(Totals totals) => new HideShopPrices();

            public string SuffixOf(ItemPrice price) => "";
        }
    }
}
