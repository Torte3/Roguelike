#nullable enable
using System;
using System.Collections.Generic;
using R3;
using TMPro;
using Unity.Logging;
using UnityEngine;
using Utilities;

namespace View.UI.Inventory
{
    public class InventoryView : MonoBehaviour
    {
        [SerializeField] private StorageView _storageView;
        [SerializeField] private TMP_Text _infoText;
        private readonly ReactiveProperty<int> _focusIndex = new(0);
        private List<InventoryViewIndex> _lockedItemIndexes = new();
        private bool _enabled = true;
        private bool _isFollowingFocus;
        public ReadOnlyReactiveProperty<InventoryViewIndex> Focus => _focusIndex.Select(index => GetFocus(index)).ToReadOnlyReactiveProperty();
        // 現在のフォーカス（単一所有者）。ReactiveProperty を生成せず即値で返す。
        public InventoryViewIndex CurrentFocus => GetFocus(_focusIndex.CurrentValue);
        private void FollowFocus()
        {
            Log.Debug($"[View]InventoryView FollowFocus");
            _isFollowingFocus = true;

            _storageView.OnSelected
                .Subscribe(index =>
                {
                    _focusIndex.Value = index;
                });

            Focus.Subscribe(_ =>
            {
                UpdateInfoText();
            });
        }
        public void Reset(List<IItemRow> itemDataList, IItemRow underfootRow, IItemRow bareHandRow)
        {
            Log.Debug($"[View]InventoryView Clear");
            var itemDataListAndEtc = new List<IItemRow>(itemDataList)
            {
                underfootRow,
                bareHandRow
            };
            _storageView.Reset(itemDataListAndEtc);
            MoveFocusTo(Math.Min(_focusIndex.Value, _storageView.ItemViews.Count - 1));
            if (!_isFollowingFocus)
                FollowFocus();
        }

        public void ResetFocus()
        {
            MoveFocusTo(0);
        }

        private void MoveFocusTo(int index)
        {
            _focusIndex.Value = index;
            _storageView.Select(index);
            UpdateInfoText();
        }

        public void Insert(int index, IItemRow itemData)
        {
            Log.Debug($"[View]InventoryView Insert Index: {index}");
            _storageView.Insert(itemData, index, true);
            if (_focusIndex.Value >= index)
            {
                _focusIndex.Value++;
            }
        }
        public void Remove(int index)
        {
            Log.Debug($"[View]InventoryView Remove Index: {index}");
            _storageView.Remove(index);
            if (_focusIndex.Value > index)
            {
                _focusIndex.Value--;
            }
            else if (_focusIndex.Value == index)
            {
                _storageView.Select(index);
            }
        }
        public void Replace(int index, IItemRow itemData)
        {
            Log.Debug($"[View]InventoryView Replace Index: {index}");
            _storageView.Replace(itemData, index, true);
            if (Focus.CurrentValue.Index == index)
                UpdateInfoText();
        }
        public void UpdateGroundItem(IItemRow itemData)
        {
            Log.Debug($"[View]InventoryView UpdateGroundItem");
            _storageView.Replace(itemData, _storageView.ItemViews.Count - 2, true);
            if (Focus.CurrentValue.IsOnGroundItem)
                UpdateInfoText();
        }
        private InventoryViewIndex GetFocus(int index)
        {
            if (index == _storageView.ItemViews.Count - 2)
                return InventoryViewIndex.GroundItem;
            else if (index == _storageView.ItemViews.Count - 1)
                return InventoryViewIndex.Empty;
            else
                return new InventoryViewIndex(index);
        }

        private int GetIndex(InventoryViewIndex index)
        {
            Log.Verbose($"[View]InventoryView GetIndex: {index}");
            if (index.IsOnGroundItem)
                return _storageView.ItemViews.Count - 2;
            else if (index.IsOnEmpty)
                return _storageView.ItemViews.Count - 1;
            else
                return index.Index;
        }

        private void UpdateInfoText()
        {
            Log.Debug($"[View]InventoryView UpdateInfoText: {Focus.CurrentValue}");
            var item = _storageView.GetItem(_focusIndex.CurrentValue);
            _infoText.text = item.Info;
        }

        public void SetCanSkip(bool canSkip)
        {
            _storageView.UpdateItemSkip(canSkip);
        }

        public void ConfigureNavigation(Func<Vector2> readNavigate)
        {
            _storageView.ConfigureNavigation(readNavigate);
        }

        private void UpdateAllItemInteractable()
        {
            foreach (var (item, index) in _storageView.ItemViews.Index())
            {
                var viewIndex = GetFocus(index);
                var interactable = _enabled && !_lockedItemIndexes.Contains(viewIndex) && item.ItemData.CanSelect;
                _storageView.UpdateItemInteractable(index, interactable);
            }
        }

        public void LockItems(List<InventoryViewIndex> lockedItemIndexes)
        {
            Log.Debug($"[View]InventoryView LockItems: Count: {lockedItemIndexes.Count}");
            _lockedItemIndexes.AddRange(lockedItemIndexes);
            UpdateAllItemInteractable();
        }

        public void UnlockAllItems()
        {
            Log.Debug($"[View]InventoryView UnlockAllItems");
            _lockedItemIndexes.Clear();
            UpdateAllItemInteractable();
        }

        internal void EnableAllItems()
        {
            Log.Debug($"[View]InventoryView EnableAllItems");
            if (_enabled)
                return;
            _enabled = true;
            UpdateAllItemInteractable();
        }

        internal void DisableAllItems()
        {
            Log.Debug($"[View]InventoryView DisableAllItems");
            if (!_enabled)
                return;
            _enabled = false;
            UpdateAllItemInteractable();
        }
    }
}