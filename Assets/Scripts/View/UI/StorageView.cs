#nullable enable
using System.Collections.Generic;
using R3;
using Unity.Logging;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace View.UI
{
    public class StorageView : MonoBehaviour
    {
        [SerializeField] private InventoryItemView _itemViewPrefab;
        private readonly Subject<int> _onSelected = new();
        private readonly List<InventoryItemView> _itemViews = new();
        public IReadOnlyList<InventoryItemView> ItemViews => _itemViews;
        public Observable<int> OnSelected => _onSelected;
        private bool _canSkip = false;

        public ItemViewData GetItem(int index)
        {
            Log.Verbose($"[View]StorageView GetItem: {index}");
            return _itemViews[index].ItemData;
        }

        public int GetIndex(InventoryItemView itemView)
        {
            return _itemViews.IndexOf(itemView);
        }

        private void UpdateVerticalNavigation()
        {
            Log.Verbose($"[View]StorageView UpdateNavigation");
            foreach (var (view, index) in _itemViews.Index())
            {
                var nav = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnLeft = FindInteractableItem(index, -1, 10),
                    selectOnRight = FindInteractableItem(index, 1, 10),
                    selectOnUp = FindInteractableItem(index, -1),
                    selectOnDown = FindInteractableItem(index, 1)
                };
                view.GetComponent<Selectable>().navigation = nav;
            }
        }

        private Selectable? FindInteractableItem(int currentIndex, int direction, int distance = 1)
        {
            if (_itemViews.Count == 0) return null;

            var startIndex = currentIndex;
            var index = (currentIndex + direction * distance).Mod(_itemViews.Count);

            while (index != startIndex)
            {
                if (!_canSkip || _itemViews[index].interactable)
                {
                    return _itemViews[index].GetComponent<Selectable>();
                }
                index = (index + direction).WrapIndex(_itemViews.Count);
            }

            return null;
        }

        public void Reset(List<ItemViewData> itemDatas)
        {
            Log.Debug($"[View]StorageView Reset (Capacity: {ItemViews.Count})");
            var desiredCount = itemDatas.Count;
            var currentCount = _itemViews.Count;

            var reuseCount = Mathf.Min(currentCount, desiredCount);
            for (int i = 0; i < reuseCount; i++)
            {
                Replace(itemDatas[i], i, true);
            }

            if (currentCount > desiredCount)
            {
                for (int i = currentCount - 1; i >= desiredCount; i--)
                {
                    Remove(i);
                }
            }
            else if (desiredCount > currentCount)
            {
                for (int i = currentCount; i < desiredCount; i++)
                {
                    Insert(itemDatas[i], i, true);
                }
            }

            UpdateVerticalNavigation();
            UpdateSiblingOrder();
        }

        public void Clear()
        {
            Log.Debug($"[View]StorageView Clear (Capacity: {ItemViews.Count})");
            foreach (var view in _itemViews.WhereNotNull())
                Destroy(view.gameObject);
            _itemViews.Clear();
        }

        public void Select(int index)
        {
            Log.Verbose($"[View]StorageView Select: {index}");
            if (_itemViews[index].interactable)
                _itemViews[index].Select();
        }

        public void UpdateSiblingOrder()
        {
            for (int i = 0; i < _itemViews.Count; i++)
            {
                _itemViews[i].transform.SetSiblingIndex(i);
            }
        }

        public void Insert(ItemViewData itemData, int index, bool interactable)
        {
            Log.Verbose($"[View]StorageView Insert: {index}");
            var view = Instantiate(_itemViewPrefab, transform);
            _itemViews.Insert(index, view);
            view.Set(itemData);
            view.UpdateInteractable(interactable);
            view.OnSelected.Subscribe(_ => _onSelected.OnNext(GetIndex(view))).AddTo(view);
            UpdateVerticalNavigation();
            UpdateSiblingOrder();
        }

        public void Remove(int index)
        {
            Log.Verbose($"[View]StorageView Remove: {index}");
            Destroy(ItemViews[index].gameObject);
            _itemViews.RemoveAt(index);
            UpdateVerticalNavigation();
            UpdateSiblingOrder();
        }

        public void Replace(ItemViewData itemData, int index, bool interactable)
        {
            Log.Verbose($"[View]StorageView Replace: {index}");
            var view = _itemViews[index];
            view.Set(itemData);
            view.UpdateInteractable(interactable);
        }

        public void UpdateItemInteractable(int index, bool interactable)
        {
            Log.Verbose($"[View]StorageView UpdateItemState: {index}");
            var view = _itemViews[index];
            view.UpdateInteractable(interactable);
        }

        public void UpdateItemSkip(bool canSkip)
        {
            Log.Verbose($"[View]StorageView UpdateItemSkip: {canSkip}");
            _canSkip = canSkip;
            UpdateVerticalNavigation();
        }
    }
}