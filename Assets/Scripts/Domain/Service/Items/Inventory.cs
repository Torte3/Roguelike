#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Message;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Memento;
using R3;
using Utilities;

namespace Domain.Service.Items
{
    internal class Inventory : IInventory, IDisposable, ISerializable<StorageMemento>
    {
        private readonly Storage _storage;

        private readonly CompositeDisposable _disposables = new();
        private readonly Dictionary<IItem, CompositeDisposable> _itemDisposables = new();

        private ICharacter _character;

        public IEnumerable<IItem> AllItems => _storage.AllItems;
        public IEnumerable<(IItem Item, int Index)> AllItemsWithIndex => _storage.AllItemsWithIndex;
        public ReadOnlyReactiveProperty<int> CurrentItemCount => _storage.CurrentItemCount;
        public ReadOnlyReactiveProperty<int> Capacity => _storage.Capacity;
        public bool CanAddItem => _storage.CanAddItem;
        public bool CanRemoveItem => _storage.CanRemoveItem;
        public Observable<OnItemInserted> OnItemInserted => _storage.OnItemInserted;
        public Observable<OnItemRemoved> OnItemRemoved => _storage.OnItemRemoved;
        public Observable<OnItemReplaced> OnItemReplaced => _storage.OnItemReplaced;
        public Observable<OnItemUpdated> OnItemUpdated => _storage.OnItemUpdated;

        public Inventory(StorageMemento data, ICharacter character)
        {
            _storage = new Storage(data);
            _character = character;

            foreach (var item in _storage.AllItems)
            {
                _itemDisposables[item] = new CompositeDisposable();

                item.IsPassiveActive.SkipLatestValueOnSubscribe().Subscribe(
                    isPassiveActive =>
                    {
                        if (isPassiveActive)
                            foreach (var condition in item.PassiveConditions)
                                condition.Inflict(_character, Id<IEntity>.Empty);
                        else
                            foreach (var condition in item.PassiveConditions)
                                condition.Delete(_character, Id<IEntity>.Empty);
                    }
                ).AddTo(_itemDisposables[item]);
            }

            _storage.OnItemInserted.Subscribe(itemChanged => InitializeAddedItem(itemChanged.NewItem));
            _storage.OnItemRemoved.Subscribe(itemChanged => InitializeRemovedItem(itemChanged.OldItem));
            _storage.OnItemReplaced.Subscribe(itemChanged =>
            {
                InitializeRemovedItem(itemChanged.OldItem);
                InitializeAddedItem(itemChanged.NewItem);
            });
        }

        private void InitializeAddedItem(IItem item)
        {
            _itemDisposables[item] = new CompositeDisposable();
            if (item.IdentifyIfGot || _character.AutoIdentify.CurrentValue)
            {
                _character.KnowItem(item, false);
            }

            if (item.IsPassiveActive.CurrentValue)
            {
                foreach (var condition in item.PassiveConditions)
                    condition.Inflict(_character, Id<IEntity>.Empty);
            }

            item.IsPassiveActive.SkipLatestValueOnSubscribe().Subscribe(
                isPassiveActive =>
                {
                    if (isPassiveActive)
                        foreach (var condition in item.PassiveConditions)
                            condition.Inflict(_character, Id<IEntity>.Empty);
                    else
                        foreach (var condition in item.PassiveConditions)
                            condition.Delete(_character, Id<IEntity>.Empty);
                }
            ).AddTo(_itemDisposables[item]);
        }

        private void InitializeRemovedItem(IItem item)
        {
            _itemDisposables[item].Clear();
            _itemDisposables.Remove(item);
            if (item.IsPassiveActive.CurrentValue)
            {
                foreach (var condition in item.PassiveConditions)
                    condition.Delete(_character, Id<IEntity>.Empty);
            }
        }

        public void Dispose()
        {
            _disposables.Dispose();
            foreach (var disposable in _itemDisposables)
                disposable.Value.Dispose();
            _itemDisposables.Clear();
        }

        public void Sort(InventorySortingMode sortingMode, ItemMarketPriceTable market)
        {
            if (sortingMode == InventorySortingMode.None)
                return;

            var items = _storage.AllItems.ToList();
            if (items.Count <= 1)
                return;

            var sortedItems = sortingMode switch
            {
                InventorySortingMode.ByCategory => items.OrderBy(item => ((BaseItem)item).Category).ThenBy(item => item.BaseName).ToList(),
                InventorySortingMode.ByPrice => items.OrderBy(item => item.GetPrice(market)).ThenBy(item => ((BaseItem)item).Category).ToList(),
                _ => items
            };

            for (int i = 0; i < sortedItems.Count; i++)
            {
                var targetItem = sortedItems[i];
                var currentIndex = _storage.GetItemIndex(targetItem);

                if (!currentIndex.HasValue)
                    throw new Exception($"Item {targetItem.DebugName} not found in inventory");

                if (currentIndex.Value != i)
                {
                    _storage.Swap(currentIndex.Value, i);
                }
            }
        }

        public void UpdateTurn()
        {
            foreach (var item in AllItems)
            {
                item.UpdateTurn();
            }
        }

        public StorageMemento Serialize() => _storage.Serialize();
        public bool HasEmptySpace() => _storage.HasEmptySpace();
        public bool Contains(IItem item) => _storage.Contains(item);
        public bool Contains(string baseName) => _storage.Contains(baseName);
        public int? GetItemIndex(IItem item) => _storage.GetItemIndex(item);
        public bool HasItem(IItem item) => _storage.HasItem(item);
        public bool HasItemAt(int index) => _storage.HasItemAt(index);
        public bool HasItemAt(int index, out IItem item) => _storage.HasItemAt(index, out item);
        public IItem? GetItem(int index) => _storage.GetItem(index);
        public bool CanAddToEmpty() => _storage.CanAddToEmpty();
        public bool CanAddIgnoreEmptySpace() => _storage.CanAddIgnoreEmptySpace();
        public void AddToEmpty(IItem item)
        {
            if (_storage.CanAddToEmpty())
            {
                _storage.AddToEmpty(item);
                if (item.IdentifyIfGot || _character.AutoIdentify.CurrentValue)
                {
                    _character.KnowItem(item, false);
                }
            }
            else
                throw new Exception("Can't add item to inventory");
        }
        public bool CanAddOrNot(IItem? item)
        {
            if (item == null)
                return true;
            return CanAddToEmpty();
        }
        public void AddOrNot(IItem? item)
        {
            if (item == null)
                return;
            AddToEmpty(item);
        }
        public bool CanInsert(int index) => _storage.CanInsert(index);
        public void Insert(IItem item, int index)
        {
            if (_storage.CanInsert(index))
            {
                _storage.Insert(item, index);

            }
            else
                throw new Exception("Can't insert item to inventory");
        }
        public bool CanRemove(int index) => _storage.CanRemove(index);
        public IItem Remove(int index) => _storage.Remove(index);
        public bool CanRemove(IItem item) => _storage.CanRemove(item);
        public void Remove(IItem item) => _storage.Remove(item);
        public bool CanRemove(string baseName) => _storage.CanRemove(baseName);
        public void Remove(string baseName) => _storage.Remove(baseName);
        public bool CanReplace(int index) => _storage.CanReplace(index);
        public IItem Replace(IItem item, int index)
        {
            if (_storage.CanReplace(index))
            {
                if (item.IdentifyIfGot || _character.AutoIdentify.CurrentValue)
                {
                    _character.KnowItem(item, false);
                }
                var replacedItem = _storage.Replace(item, index);
                return replacedItem;
            }
            else
                throw new Exception("Can't replace item in inventory");
        }
        public void Replace(IItem oldItem, IItem newItem)
        {
            var index = GetItemIndex(oldItem).Value;
            _storage.Replace(newItem, index);
        }
        public bool CanSwap(int index1, int index2) => _storage.CanSwap(index1, index2);
        public void Swap(int index1, int index2) => _storage.Swap(index1, index2);
        public bool CanReplaceOrRemove(IItem? item, int index)
        {
            if (item == null)
                return CanRemove(index);
            return CanReplace(index);
        }
        public IItem ReplaceOrRemove(IItem? item, int index)
        {
            if (item == null)
                return Remove(index);
            return Replace(item, index);
        }
        public IEnumerable<IItem> Clear() => _storage.Clear();
    }
}