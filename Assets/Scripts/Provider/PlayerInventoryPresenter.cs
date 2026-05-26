#nullable enable
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Dungeon;
using Domain.Model.Item;
using Domain.Model.Map;
using Game;
using ObservableCollections;
using R3;
using UnityEngine;
using Utilities;
using VContainer;
using View.UI;

namespace Provider
{

    public class PlayerInventoryPresenter
    {
        private readonly CompositeDisposable _disposables = new();

        [Inject]
        public PlayerInventoryPresenter(GameManager gameManager, World world, InventoryView inventoryView)
        {
            inventoryView.Initialize();
            world.OnActiveMapChanged.Subscribe(mapChanged =>
                {
                    _disposables.Clear();

                    var map = mapChanged.Map;
                    var inventory = map.Player.Character.Inventory;

                    inventoryView.Reset(
                        inventory.AllItems.Select(
                            item => BuildItemViewData(map, item, canSelect: true)
                        ).ToList(),
                        mapChanged.IsNewWorld);

                    if (map.Shop != null)
                    {
                        map.Shop.IsInside
                            .DistinctUntilChanged()
                            .Subscribe(_ => ReplaceAllItemViews(inventoryView, map))
                            .AddTo(_disposables);
                    }

                    inventory.OnItemInserted.Subscribe(inserted =>
                    {
                        InsertItemView(inventoryView, map, inserted.NewItem, inventory.CanRemoveItem, inserted.Index);
                    }).AddTo(_disposables);

                    inventory.OnItemRemoved.Subscribe(removed =>
                    {
                        RemoveItemView(inventoryView, removed.Index);
                    }).AddTo(_disposables);

                    inventory.OnItemReplaced.Subscribe(replaced =>
                    {
                        ReplaceItemView(inventoryView, map, replaced.NewItem, inventory.CanRemoveItem, replaced.Index);
                    }).AddTo(_disposables);

                    inventory.OnItemUpdated.Subscribe(itemUpdated =>
                    {
                        var index = inventory.GetItemIndex(itemUpdated.Item);
                        if (index == null)
                            return;
                        ReplaceItemView(inventoryView, map, itemUpdated.Item, inventory.CanRemoveItem, index.Value);
                    }).AddTo(_disposables);

                    gameManager.OnTurnChanged.Subscribe(_ =>
                    {
                        UpdateGroundItemView(inventoryView, map);
                    }).AddTo(_disposables);

                    Observable.Merge(
                        map.Player.Character.KnownItemNames.ObserveChanged().AsUnitObservable(),
                        map.ItemPlaceholders.OnItemRenamed
                    ).Subscribe(_ =>
                    {
                        ReplaceAllItemViews(inventoryView, map);
                    }).AddTo(_disposables);

                    ReplaceAllItemViews(inventoryView, map);
                }
            );
        }

        private void ReplaceAllItemViews(InventoryView inventoryView, IMap map)
        {
            var inventory = map.Player.Character.Inventory;
            foreach (var (item, index) in inventory.AllItemsWithIndex)
            {
                ReplaceItemView(inventoryView, map, item, inventory.CanRemoveItem, index);
            }
            UpdateGroundItemView(inventoryView, map);
        }

        private void InsertItemView(InventoryView inventoryView, IMap map, IItem item, bool canSelect, int index)
        {
            inventoryView.Insert(
                index,
                BuildItemViewData(map, item, canSelect)
            );
        }

        private void RemoveItemView(InventoryView inventoryView, int index)
        {
            inventoryView.Remove(index);
        }

        private void ReplaceItemView(InventoryView inventoryView, IMap map, IItem item, bool canSelect, int index)
        {
            inventoryView.Replace(
                index,
                BuildItemViewData(map, item, canSelect)
            );
        }

        private IItem? GetGroundItem(IMap map)
        {
            return map.Items.At(map.Player.Character.Entity.CurrentPosition).FirstOrDefault()?.Item;
        }

        private void UpdateGroundItemView(InventoryView inventoryView, IMap map)
        {
            var item = GetGroundItem(map);
            if (item == null)
                inventoryView.UpdateGroundItem(null);
            else
                inventoryView.UpdateGroundItem(BuildItemViewData(map, item, canSelect: true));
        }

        private ItemViewData BuildItemViewData(IMap map, IItem item, bool canSelect)
        {
            var baseName = item.GetName(map.Player, map.ItemPlaceholders);
            var name = baseName + GetShopPriceSuffix(map, item);
            var showEquippedBadge = item.IsEquipped.UnwrapOr(false);
            int? count = item.IsEquipped.IsNone && item.HasActivatableSkill
                ? item.RemainingUses.CurrentValue
                : null;
            return new ItemViewData(
                name,
                item.CanAttemptUseOrThrow,
                item.Icon,
                canSelect,
                count,
                showEquippedBadge,
                item.IsCursed,
                item.IsShiny,
                map.Player.Character.IsKnownItem(item),
                item.IsCurseIdentified,
                item.Info(map.Player, map.ItemPlaceholders)
            );
        }

        private string GetShopPriceSuffix(IMap map, IItem item)
        {
            if (map.Shop == null || !map.Shop.IsInside.CurrentValue)
                return "";

            var basePrice = item.GetPrice(map.MarketPriceTable);
            if (item.State == ItemState.ShopItem)
            {
                var purchasePrice = map.Player.Character.Status.IsFlagStat(FlagStatType.Negotiator)
                    ? Mathf.RoundToInt(basePrice / 2f)
                    : basePrice;
                return $"\n{purchasePrice}G".SetColored(Colors.MediumSeaGreen);
            }
            else
            {
                var salePrice = Mathf.RoundToInt(basePrice / 2f);
                return $"\n{salePrice}G".SetColored(Colors.LightSteelBlue);
            }
        }
    }
}
