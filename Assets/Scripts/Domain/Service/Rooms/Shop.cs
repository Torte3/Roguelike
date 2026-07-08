#nullable enable
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.WorldEvents;
using Domain.Service.Events;
using Domain.Service.Items;
using UnityEngine;
using Utilities;

namespace Domain.Service.Rooms
{
    public class Shop : Room<ShopMemento>, IReadOnlyShop
    {
        private readonly ICharacter _clerk;
        private record ShopItemCache(Id<IItem> Id, int Price);

        private HashSet<ShopItemCache> _shopItems = new();
        private bool _isStolen;
        public bool IsStolen => _isStolen;
        public Id<IEntity> ClerkId => _clerk.Entity.Id;

        public bool ClerkAppearsInteractable(IMap map)
        {
            return ((IEntity)_clerk).AppearsInteractable(map);
        }

        public Shop(ShopMemento data, ICharacter clerk, IMap map) : base(data.Room,
            map.Player.Character.Entity.CurrentPosition)
        {
            _clerk = clerk;
            clerk.AddEvent(new PlayerEvent(
                new List<PlayerChoiceEvent>
                {
                    new(
                        "代金を支払う",
                        (player, map) => CanExecute && (GetSalePrice(map) > 0 || GetPurchasePrice(map) > 0),
                        (_, map) =>
                        {
                            Purchase(map);
                            return UniTask.CompletedTask;
                        }
                    )
                }
            ));

            _shopItems = data.Items.Select(item => new ShopItemCache(item.Id, item.Price)).ToHashSet();
            if (data.IsStolen)
            {
                _isStolen = true;
                CanExecute = false;
                MarkItemsAsStolen(map);
            }
        }

        public static ShopMemento Build(RectInt rect, Id<IEntity> clerkId, List<ItemEntityMemento> items, ItemMarketPriceTable market)
        {
            return new ShopMemento
            (
                new RoomMemento
                (
                    rect,
                    false,
                    false
                ),
                clerkId,
                items.Select(itemMemento =>
                {
                    var item = itemMemento.Item.Deserialize();
                    return new ShopItemMemento
                    (
                        item.Id,
                        item.GetPrice(market)
                    );
                }).ToList(),
                false
            );
        }

        public override ShopMemento Serialize()
        {
            return new ShopMemento
            (
                new RoomMemento
                (
                    Rect,
                    hasEntered,
                    hasEverEntered
                ),
                _clerk.Entity.Id,
                _shopItems.Select(item => new ShopItemMemento
                (
                    item.Id,
                    item.Price
                )).ToList(),
                _isStolen
            );
        }

        private IEnumerable<IItem> GetWritableItemsInRoom(IMap map)
        {
            return map.Items.In(Rect.RectRange()).Select(item => item.Item);
        }

        private IEnumerable<IReadOnlyItem> GetItemsInRoom(IReadOnlyMap map)
        {
            return map.ItemsIn(Rect.RectRange());
        }

        private void SetShopItems(IMap map, IEnumerable<IItem> items)
        {
            _shopItems = items.Select(item => new ShopItemCache(item.Id, item.GetPrice(map.MarketPriceTable))).ToHashSet();
            foreach (var item in items)
            {
                item.SetState(ItemState.ShopItem);
            }
        }

        private void RemoveMark(IMap map, IEnumerable<ShopItemCache> items)
        {
            foreach (var item in items)
            {
                map.GetItemByIdFromWorldOrInventory(item.Id)?.SetState(ItemState.None);
            }
        }

        private void MarkItemsAsStolen(IMap map)
        {
            foreach (var item in _shopItems)
            {
                map.GetItemByIdFromWorldOrInventory(item.Id)?.SetState(ItemState.Stolen);
            }
        }

        private IEnumerable<ShopItemCache> GetMissingItems(IReadOnlyMap map)
        {
            var itemsInRoom = GetItemsInRoom(map).Where(item => item.State == ItemState.ShopItem);
            var purchaseItems = _shopItems.Except(itemsInRoom.Select(item => new ShopItemCache(item.Id, item.GetPrice(map.MarketPriceTable))));
            return purchaseItems;
        }

        public int GetPurchasePrice(IReadOnlyMap map)
        {
            return PurchasePriceOf(GetMissingItems(map).Sum(item => item.Price), map);
        }

        private IEnumerable<ShopItemCache> GetAddedItems(IReadOnlyMap map)
        {
            var saleItems = GetItemsInRoom(map).Where(item => item.State != ItemState.ShopItem);
            return saleItems.Select(item => new ShopItemCache(item.Id, item.GetPrice(map.MarketPriceTable)));
        }

        public int GetSalePrice(IReadOnlyMap map)
        {
            return SalePriceOf(GetAddedItems(map).Sum(item => item.Price));
        }

        public int GetPrice(IReadOnlyItem item, IReadOnlyMap map)
        {
            var basePrice = item.GetPrice(map.MarketPriceTable);
            return item.State == ItemState.ShopItem ? PurchasePriceOf(basePrice, map) : SalePriceOf(basePrice);
        }

        private static int PurchasePriceOf(int basePrice, IReadOnlyMap map)
        {
            return map.PlayerCharacter.Status.IsFlagStat(FlagStatType.Negotiator)
                ? Mathf.RoundToInt(basePrice / 2f)
                : basePrice;
        }

        private static int SalePriceOf(int basePrice)
        {
            return Mathf.RoundToInt(basePrice / 2f);
        }

        private void Purchase(IMap map)
        {
            if (map.Player.Money + GetSalePrice(map) >= GetPurchasePrice(map))
            {
                var received = GetSalePrice(map);
                var paid = GetPurchasePrice(map);
                if (received > 0)
                    map.Player.AddMoney(received);
                if (paid > 0)
                    map.Player.ReduceMoney(paid);
                var purchaseItems = GetMissingItems(map);
                RemoveMark(map, purchaseItems);
                SetShopItems(map, GetWritableItemsInRoom(map));
                map.Events.Record(new ShopSettled(map.Player.Character.Label, received, paid, map.Player.Money,
                    map.Player.Character.WholeInventoryLookIn(map), map.PlayerUnderfoot(), map.ShopLookIn()));
            }
            else
            {
                map.Events.Record(new ShopPaymentRefused(map.Player.Character.Label, GetPurchasePrice(map) - GetSalePrice(map)));
            }
        }

        private void Stolen(IMap map)
        {
            map.Player.RecordSteal();
            _clerk.Affiliation.AddForceAffiliation(map.Player.Character.Entity.Id, AffiliationType.Enemy);
            _clerk.AddCondition(
                Id<IEntity>.Empty,
                ObjectLoader.Load<ConditionTemplate>("店員の怒り")
            );
            MarkItemsAsStolen(map);
            CanExecute = false;
            _isStolen = true;
            map.Events.Record(new TheftDetected(map.Player.Character.WholeInventoryLookIn(map), map.PlayerUnderfoot(), this.LookIn(map)));
        }

        protected override async UniTask EveryTimeEnter(IGameManager gameManager, IMap map)
        {
            map.Events.Record(new ShopEntered(map.Player.Character.WholeInventoryLookIn(map), map.PlayerUnderfoot(), map.ShopLookIn()));
            // 初めて店に入ったときにチュートリアルを表示する。
            await gameManager.ShowTutorialIfNeeded(TutorialType.Shop);
        }

        protected override UniTask EveryTimeExit(IGameManager gameManager, IMap map)
        {
            map.Events.Record(new ShopExited(map.Player.Character.WholeInventoryLookIn(map), map.PlayerUnderfoot(), this.LookIn(map)));
            return UniTask.CompletedTask;
        }

        protected override UniTask UpdateTurnIfNotInside(IGameManager gameManager, IMap map)
        {
            var missingItems = GetMissingItems(map);
            if (missingItems.Any())
            {
                Stolen(map);
            }

            return UniTask.CompletedTask;
        }
    }
}