#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Character.Message;
using Domain.Model.Item;
using Domain.Model.Memento;
using ObservableCollections;
using R3;
using Utilities;

namespace Domain.Service.Items
{
    internal class Storage : IStorage, IDisposable, ISerializable<StorageMemento>
    {
        private readonly ReactiveProperty<int> _currentItemCount = new();
        public ReadOnlyReactiveProperty<int> CurrentItemCount => _currentItemCount;
        private readonly ReactiveProperty<int> _capacity = new();
        public ReadOnlyReactiveProperty<int> Capacity => _capacity;
        private readonly ObservableList<IItem> _items;
        public IEnumerable<IItem> AllItems => _items
            .WhereNotNull();
        public IEnumerable<(IItem Item, int Index)> AllItemsWithIndex => _items
            .Select((item, index) => (item, index))
            .Where(x => x.item != null)
            .Cast<(IItem, int)>();
        public bool CanAddItem { get; init; }
        public bool CanRemoveItem { get; init; }
        public Observable<OnItemInserted> OnItemInserted => _items.ObserveAdd().Select(itemAdded =>
            new OnItemInserted(itemAdded.Value, itemAdded.Index)
        );
        public Observable<OnItemRemoved> OnItemRemoved => _items.ObserveRemove().Select(itemRemoved =>
            new OnItemRemoved(itemRemoved.Value, itemRemoved.Index)
        );
        public Observable<OnItemReplaced> OnItemReplaced => _items.ObserveReplace().Select(itemChanged =>
            new OnItemReplaced(itemChanged.NewValue, itemChanged.OldValue, itemChanged.Index)
        );
        private readonly Subject<OnItemUpdated> _onItemUpdated = new();
        public Observable<OnItemUpdated> OnItemUpdated => _onItemUpdated;
        private readonly CompositeDisposable _disposables = new();
        private readonly Dictionary<IItem, CompositeDisposable> _itemDisposables = new();

        public Storage(StorageMemento data)
        {
            _currentItemCount.Value = data.Items.Count;
            _capacity = new ReactiveProperty<int>(data.Capacity);
            _items = new ObservableList<IItem>(data.Items.Select(x => x.Deserialize()));
            CanAddItem = data.CanAddItem;
            CanRemoveItem = data.CanRemoveItem;
            _items.SubscribeIncludingCurrentItems(
                itemAdded =>
                {
                    var index = _items.IndexOf(itemAdded);
                    _itemDisposables[itemAdded] = new CompositeDisposable();
                    if (itemAdded != null)
                    {
                        itemAdded.OnItemUpdated.Subscribe(
                            _ => _onItemUpdated.OnNext(new OnItemUpdated(itemAdded))
                        ).AddTo(_itemDisposables[itemAdded]);

                        itemAdded.RemainingUses.SkipLatestValueOnSubscribe().Subscribe(
                            remainingUses =>
                            {
                                if (remainingUses <= 0 && itemAdded.AutoDestroyWhenDisabled)
                                {
                                    ForceRemove(itemAdded);
                                }
                            }
                        ).AddTo(_itemDisposables[itemAdded]);

                        itemAdded.OnMimicRevealed.Subscribe(
                            _ =>
                            {
                                ForceRemove(itemAdded);
                            }
                        ).AddTo(_itemDisposables[itemAdded]);
                    }
                },
                itemRemoved =>
                {
                    _itemDisposables[itemRemoved].Dispose();
                    _itemDisposables.Remove(itemRemoved);
                }).AddTo(_disposables);
            _items.ObserveCountChanged().Subscribe(count =>
            {
                _currentItemCount.Value = count;
            }).AddTo(_disposables);
        }

        public void Dispose()
        {
            _onItemUpdated.Dispose();
            _disposables.Dispose();
            foreach (var disposable in _itemDisposables)
                disposable.Value.Dispose();
            _itemDisposables.Clear();
        }

        public StorageMemento Serialize()
        {
            return new StorageMemento
            (
                _capacity.CurrentValue,
                _items.Select(x => x.Serialize()).ToList(),
                CanAddItem,
                CanRemoveItem
            );
        }

        public static StorageMemento Build(int capacity, List<IItemMemento> items, bool canAddItemsWithStorage, bool canRemoveItem)
        {
            return new StorageMemento(capacity, items, canAddItemsWithStorage, canRemoveItem);
        }

        public bool HasEmptySpace()
        {
            return _items.Count < _capacity.CurrentValue;
        }

        public bool HasItem(IItem item)
        {
            return _items.IndexOf(item) >= 0;
        }

        public bool HasItemAt(int index)
        {
            return index < _capacity.CurrentValue;
        }

        public bool HasItemAt(int index, out IItem item)
        {
            item = GetItem(index);
            return index < _capacity.CurrentValue;
        }

        public bool Contains(IItem item)
        {
            return _items.IndexOf(item) >= 0;
        }

        public bool Contains(string baseName)
        {
            return _items.Any(x => x.BaseName == baseName);
        }

        public IItem? GetItem(int index)
        {
            return index < _items.Count ? _items[index] : null;
        }

        public int? GetItemIndex(IItem item)
        {
            var index = _items.IndexOf(item);
            return index >= 0 ? index : null;
        }

        public bool CanAddToEmpty()
        {
            return CanAddItem && HasEmptySpace();
        }

        public bool CanAddIgnoreEmptySpace()
        {
            return CanAddItem;
        }

        public bool CanInsert(int index)
        {
            return CanAddItem && HasEmptySpace() && index >= 0 && index < _capacity.CurrentValue;
        }

        public bool CanRemove(int index)
        {
            return CanRemoveItem && GetItem(index) != null;
        }

        public bool CanRemove(IItem item)
        {
            return CanRemoveItem && Contains(item);
        }

        public bool CanRemove(string baseName)
        {
            return CanRemoveItem && Contains(baseName);
        }

        public bool CanReplace(int index)
        {
            return CanAddIgnoreEmptySpace() && CanRemove(index);
        }

        public void AddToEmpty(IItem item)
        {
            if (!CanAddToEmpty())
                throw new Exception("Can't add item to storage");
            _items.Add(item);
        }

        public void Insert(IItem item, int index)
        {
            if (!CanInsert(index))
                throw new Exception("Can't insert item to storage");
            _items.Insert(index, item);
        }

        public IItem Remove(int index)
        {
            if (!CanRemove(index))
                throw new Exception("Can't remove item from storage");
            var removed = GetItem(index);
            _items.RemoveAt(index);
            return removed;
        }

        public void Remove(IItem item)
        {
            if (!CanRemove(item))
                throw new Exception("Can't remove item from storage");
            _items.Remove(item);
        }

        public void Remove(string baseName)
        {
            if (!CanRemove(baseName))
                throw new Exception("Can't remove item from storage");
            var item = _items.First(x => x.BaseName == baseName);
            _items.Remove(item);
        }

        public IItem Replace(IItem item, int index)
        {
            if (!CanReplace(index))
                throw new Exception("Can't replace item from storage");
            var removed = Remove(index);
            Insert(item, index);
            return removed;
        }

        public void Replace(IItem oldItem, IItem newItem)
        {
            var index = GetItemIndex(oldItem).Value;
            if (!CanReplace(index))
                throw new Exception("Can't replace item from storage");
            Replace(newItem, index);
        }

        public IItem? ForceRemove(int index)
        {
            var removed = GetItem(index);
            _items.RemoveAt(index);
            return removed;
        }

        public void ForceRemove(IItem item)
        {
            _items.Remove(item);
        }

        public bool CanSwap(int index1, int index2)
        {
            var item1 = GetItem(index1);
            var item2 = GetItem(index2);
            if (item1 == null || item2 == null)
                return false;
            return CanAddItem && CanRemoveItem;
        }

        public void Swap(int index1, int index2)
        {
            if (!CanSwap(index1, index2))
                throw new Exception("Can't swap items in storage");
            var item1 = ForceRemove(index1);
            var adjustedIndex2 = index2;
            if (index2 > index1)
                adjustedIndex2--;
            var item2 = Replace(item1, adjustedIndex2);
            Insert(item2, index1);
        }

        public IEnumerable<IItem> Clear()
        {
            var count = _items.Count;
            for (int i = 0; i < count; i++)
            {
                var item = ForceRemove(0);
                if (item != null)
                    yield return item;
            }
        }
    }
}