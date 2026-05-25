#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Item;
using Domain.Model.Memento;
using Domain.Service.Items;
using ObservableCollections;
using R3;
using UnityEngine;
using Utilities;
using VContainer;

namespace Game
{
    public sealed class ItemManager : IDisposable
    {
        private readonly ObservableList<IItemEntity> _items = new();
        private HashSet<Vector2Int> _allItemPositions = new();
        private CompositeDisposable _disposables = new();

        [Inject]
        public ItemManager()
        {
            _items.ObserveCountChanged().Subscribe(_ => SetAllItemPosition());
            _items.SubscribeIncludingCurrentObservables(
                item => item.Entity.Position,
                (item, position) => SetAllItemPosition()
            ).AddTo(_disposables);
            _items.SubscribeIncludingCurrentObservables(
                item => item.Item.RemainingUses.SkipLatestValueOnSubscribe(),
                (item, remainingUses) =>
                {
                    if (remainingUses <= 0 && item.Item.AutoDestroyWhenDisabled)
                    {
                        _items.Remove(item);
                    }
                }
            ).AddTo(_disposables);
            _items.SubscribeIncludingCurrentObservables(
                item => item.Entity.OnDestroyed,
                (item, dead) => _items.Remove(item)
            ).AddTo(_disposables);
        }

        public IObservableCollection<IItemEntity> Items => _items;

        public void Dispose()
        {
            _items.ForEach(item => item.Dispose());
            _disposables.Dispose();
        }

        public void AddItem(IItemEntity item)
        {
            _items.Add(item);
        }

        public IItemEntity SpawnItem(IItem item, Vector2Int spawnPosition)
        {
            var itemEntity = new ItemEntity(ItemEntity.Build(spawnPosition, item.Serialize()));
            AddItem(itemEntity);
            return itemEntity;
        }

        public IItemEntity SpawnItem(ItemEntityMemento item)
        {
            var itemEntity = new ItemEntity(item);
            AddItem(itemEntity);
            return itemEntity;
        }

        public HashSet<Vector2Int> GetAllItemPositions()
        {
            return _allItemPositions;
        }

        private void SetAllItemPosition()
        {
            _allItemPositions = Items.Positions().ToHashSet();
        }

        public IItemEntity? GetItemAt(Vector2Int position)
        {
            return _items.FirstOrDefault(item => item.Entity.CurrentPosition == position);
        }

        public bool CanPickUpAt(Vector2Int position, bool canPickUpShopItem = false)
        {
            var item = GetItemAt(position);
            if (item == null)
                return false;

            return canPickUpShopItem || item.Item.State != ItemState.ShopItem;
        }

        public IItemEntity? TryPickUpAt(Vector2Int position, bool canPickUpShopItem = false)
        {
            if (!CanPickUpAt(position, canPickUpShopItem))
                return null;

            var item = GetItemAt(position)!;
            _items.Remove(item);
            return item;
        }

        public IItemEntity PickUpAt(Vector2Int position, bool pickUpShopItem = false)
        {
            return TryPickUpAt(position, pickUpShopItem) ?? throw new Exception("item cannot be picked up");
        }
    }
}