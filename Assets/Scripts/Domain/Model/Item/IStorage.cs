#nullable enable
using System.Collections.Generic;
using Domain.Model.Character.Message;
using R3;

namespace Domain.Model.Item
{
    public interface IStorage
    {
        public IEnumerable<IItem> AllItems { get; }
        public IEnumerable<(IItem Item, int Index)> AllItemsWithIndex { get; }
        public ReadOnlyReactiveProperty<int> CurrentItemCount { get; }
        public ReadOnlyReactiveProperty<int> Capacity { get; }
        public bool CanAddItem { get; }
        public bool CanRemoveItem { get; }
        public Observable<OnItemInserted> OnItemInserted { get; }
        public Observable<OnItemRemoved> OnItemRemoved { get; }
        public Observable<OnItemReplaced> OnItemReplaced { get; }
        public Observable<OnItemUpdated> OnItemUpdated { get; }
        public bool HasEmptySpace();
        public bool HasItem(IItem item);
        public bool HasItemAt(int index);
        public bool HasItemAt(int index, out IItem item);
        public bool Contains(IItem item);
        public bool Contains(string baseName);
        public IItem? GetItem(int index);
        public int? GetItemIndex(IItem item);
        public bool CanAddToEmpty();
        public bool CanAddIgnoreEmptySpace();
        public void AddToEmpty(IItem item);
        public bool CanInsert(int index);
        public void Insert(IItem item, int index);
        public bool CanRemove(int index);
        public IItem Remove(int index);
        public bool CanRemove(IItem item);
        public void Remove(IItem item);
        public bool CanRemove(string baseName);
        public void Remove(string baseName);
        public bool CanReplace(int index);
        public IItem Replace(IItem item, int index);
        public void Replace(IItem oldItem, IItem newItem);
        public bool CanSwap(int index1, int index2);
        public void Swap(int index1, int index2);
        public IEnumerable<IItem> Clear();
    }
}