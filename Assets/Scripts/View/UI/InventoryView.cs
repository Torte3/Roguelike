#nullable enable
using System.Collections.Generic;
using R3;
using TMPro;
using Unity.Logging;
using UnityEngine;
using Utilities;

namespace View.UI
{
    public class InventoryView : MonoBehaviour
    {
        [SerializeField] private StorageView _storageView;
        [SerializeField] private TMP_Text _infoText;
        [SerializeField] private Sprite _groundItemIcon;
        [SerializeField] private Sprite _emptyIcon;
        private readonly ReactiveProperty<int> _focusIndex = new(0);
        private ItemViewData _defaultGroundItemItem;
        private ItemViewData _defaultEmptyItem;
        private List<InventoryViewIndex> _lockedItemIndexes = new();
        private bool _enabled = true;
        public ReadOnlyReactiveProperty<InventoryViewIndex> Focus => _focusIndex.Select(index => GetFocus(index)).ToReadOnlyReactiveProperty();
        public void Initialize()
        {
            Log.Debug($"[View]InventoryView Initialize");
            _defaultGroundItemItem = new ItemViewData("[足元]", false, _groundItemIcon, true, null, false, false, false, true, true,
            "アイテムを使わずに攻撃\n\n前1マスを対象に\n攻撃[物理1]\n成功率：95%");
            _defaultEmptyItem = new ItemViewData("", false, _emptyIcon, true, null, false, false, false, true, true,
            "アイテムを使わずに攻撃\n\n前1マスを対象に\n攻撃[物理1]\n成功率：95%");
            Reset(new(), true);

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
        public void Reset(List<ItemViewData> itemDataList, bool resetFocus)
        {
            Log.Debug($"[View]InventoryView Clear");
            var itemDataListAndEtc = new List<ItemViewData>(itemDataList)
            {
                _defaultGroundItemItem,
                _defaultEmptyItem
            };
            _storageView.Reset(itemDataListAndEtc);
            if (resetFocus)
                _storageView.Select(0);
            UpdateInfoText();
        }

        public void Insert(int index, ItemViewData itemData)
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
        public void Replace(int index, ItemViewData itemData)
        {
            Log.Debug($"[View]InventoryView Replace Index: {index}");
            _storageView.Replace(itemData, index, true);
            if (Focus.CurrentValue.Index == index)
                UpdateInfoText();
        }
        public void UpdateGroundItem(ItemViewData? itemData)
        {
            Log.Debug($"[View]InventoryView UpdateGroundItem");
            if (itemData != null)
                _storageView.Replace(itemData with { name = $"[足元] {itemData.name}" }, _storageView.ItemViews.Count - 2, true);
            else
                _storageView.Replace(_defaultGroundItemItem, _storageView.ItemViews.Count - 2, true);
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
            var currentIndex = _focusIndex.CurrentValue;
            var item = _storageView.GetItem(currentIndex);
            _infoText.text = item.info;
        }

        public void SetCanSkip(bool canSkip)
        {
            _storageView.UpdateItemSkip(canSkip);
        }

        public void UpdateAllItemInteractable()
        {
            foreach (var (item, index) in _storageView.ItemViews.Index())
            {
                var viewIndex = GetFocus(index);
                var interactable = _enabled && !_lockedItemIndexes.Contains(viewIndex) && item.ItemData.canSelect;
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

        public void EnableAllItems()
        {
            Log.Debug($"[View]InventoryView EnableAllItems");
            if (_enabled)
                return;
            _enabled = true;
            UpdateAllItemInteractable();
        }

        public void DisableAllItems()
        {
            Log.Debug($"[View]InventoryView DisableAllItems");
            if (!_enabled)
                return;
            _enabled = false;
            UpdateAllItemInteractable();
        }
    }
}