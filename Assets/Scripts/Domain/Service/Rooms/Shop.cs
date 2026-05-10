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
using Domain.Service.Events;
using Domain.Service.Items;
using Domain.Service.Logs;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Rooms
{
    public class Shop : Room<ShopMemento>, IShop
    {
        private readonly ICharacter _clerk;
        private record ShopItemCache(Id<IItem> Id, int Price);

        private HashSet<ShopItemCache> _shopItems = new();
        private ReactiveProperty<bool> _isStolen = new(false);
        public ReadOnlyReactiveProperty<bool> IsStolen => _isStolen;

        public Shop(ShopMemento data, ICharacter clerk, IGameManager gameManager, IMap map) : base(data.Room,
            map.Player.Character.Entity.CurrentPosition)
        {
            _clerk = clerk;
            clerk.AddEvent(new PlayerEvent(
                null,
                new List<PlayerChoiceEvent>
                {
                    new(
                        "代金を支払う",
                        (player, map) => CanExecute && (GetSalePrice(map) > 0 || GetPurchasePrice(map) > 0),
                        (gameManager, map) =>
                        {
                            Purchase(gameManager, map);
                            return UniTask.CompletedTask;
                        }
                    )
                }
            ));

            if (data.IsStolen)
            {
                Stolen(gameManager, map);
                return;
            }

            _shopItems = data.Items.Select(item => new ShopItemCache(item.Id, item.Price)).ToHashSet();
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
                _isStolen.Value
            );
        }

        private IEnumerable<IItem> GetItemsInRoom(IMap map)
        {
            return map.Items.In(Rect.RectRange()).Select(item => item.Item);
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

        private IEnumerable<ShopItemCache> GetMissingItems(IMap map)
        {
            var itemsInRoom = GetItemsInRoom(map).Where(item => item.State == ItemState.ShopItem);
            var purchaseItems = _shopItems.Except(itemsInRoom.Select(item => new ShopItemCache(item.Id, item.GetPrice(map.MarketPriceTable))));
            return purchaseItems;
        }

        public int GetPurchasePrice(IMap map)
        {
            var purchaseItems = GetMissingItems(map);
            if (map.Player.Character.Status.IsFlagStat(FlagStatType.Negotiator))
            {
                return Mathf.RoundToInt(purchaseItems.Sum(item => item.Price) / 2f);
            }

            return purchaseItems.Sum(item => item.Price);
        }

        private IEnumerable<ShopItemCache> GetAddedItems(IMap map)
        {
            var saleItems = GetItemsInRoom(map).Where(item => item.State != ItemState.ShopItem);
            return saleItems.Select(item => new ShopItemCache(item.Id, item.GetPrice(map.MarketPriceTable)));
        }

        public int GetSalePrice(IMap map)
        {
            var saleItems = GetAddedItems(map);
            return Mathf.RoundToInt(saleItems.Sum(item => item.Price) / 2f);
        }

        public void Purchase(IGameManager gameManager, IMap map)
        {
            if (map.Player.Money.CurrentValue + GetSalePrice(map) >= GetPurchasePrice(map))
            {
                if (GetSalePrice(map) > 0)
                {
                    GameLog.AddIgnoreVisibility(
                        $"{map.Player.Character.GetName(map.Player)}は<color=green>{GetSalePrice(map)}G</color>受け取った");
                    map.Player.AddMoney(GetSalePrice(map));
                }
                if (GetPurchasePrice(map) > 0)
                {
                    GameLog.AddIgnoreVisibility(
                        $"{map.Player.Character.GetName(map.Player)}は<color=yellow>{GetPurchasePrice(map)}G</color>支払った");
                    map.Player.ReduceMoney(GetPurchasePrice(map));
                }
                var purchaseItems = GetMissingItems(map);
                RemoveMark(map, purchaseItems);
                SetShopItems(map, GetItemsInRoom(map));
                gameManager.PlaySE(SE.ShopCheckout);
            }
            else
            {
                GameLog.AddIgnoreVisibility(
                    $"{map.Player.Character.GetName(map.Player)}は<color=yellow>{GetPurchasePrice(map) - GetSalePrice(map)}G</color>持っていなかった");
            }
        }

        public void Stolen(IGameManager gameManager, IMap map)
        {
            GameLog.AddIgnoreVisibility("<color=red>どろぼう！</color>");
            _clerk.Affiliation.AddForceAffiliation(map.Player.Character.Entity.Id, AffiliationType.Enemy);
            _clerk.AddCondition(
                Id<IEntity>.Empty,
                ObjectLoader.Load<ConditionTemplate>("店員の怒り")
            );
            MarkItemsAsStolen(map);
            CanExecute = false;
            _isStolen.Value = true;
            gameManager.PlayBGM(BGM.Stolen);
        }

        protected override UniTask EveryTimeEnter(IGameManager gameManager, IMap map)
        {
            gameManager.PlayBGM(BGM.Shop);
            return UniTask.CompletedTask;
        }

        protected override UniTask EveryTimeExit(IGameManager gameManager, IMap map)
        {
            gameManager.PlayBGM(BGM.Normal);
            return UniTask.CompletedTask;
        }

        protected override async UniTask UpdateTurnIfNotInside(IGameManager gameManager, IMap map)
        {
            var missingItems = GetMissingItems(map);
            if (missingItems.Any())
            {
                Stolen(gameManager, map);
                await UniTask.Delay(1000);
            }
        }
    }
}