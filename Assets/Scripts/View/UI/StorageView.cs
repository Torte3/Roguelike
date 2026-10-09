#nullable enable
using System;
using System.Collections.Generic;
using R3;
using Unity.Logging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities;

namespace View.UI
{
    public class StorageView : MonoBehaviour
    {
        [SerializeField] private InventoryItemView _itemViewPrefab;
        private readonly Subject<int> _onSelected = new();
        private readonly List<InventoryItemView> _itemViews = new();
        internal IReadOnlyList<InventoryItemView> ItemViews => _itemViews;
        internal Observable<int> OnSelected => _onSelected;
        private bool _canSkip = false;

        [Header("Navigation")]
        [SerializeField] private int _horizontalJump = 10;
        [SerializeField] private float _inputDeadzone = 0.5f;
        [SerializeField] private float _repeatDelay = 0.4f;
        [SerializeField] private float _verticalRepeatStartInterval = 0.12f;
        [SerializeField] private float _verticalRepeatMinInterval = 0.05f;
        [Tooltip("上下リピートごとに間隔へ掛ける係数 (<1で加速)")]
        [SerializeField] private float _verticalRepeatAcceleration = 0.8f;
        [Tooltip("左右リピート間隔 (加速なし・一定)")]
        [SerializeField] private float _horizontalRepeatInterval = 0.15f;

        private enum NavDir { None, Up, Down, Left, Right }
        private Func<Vector2>? _readNavigate;
        private NavDir _lastNav = NavDir.None;
        private float _repeatTimer;
        private float _repeatInterval;

        public void ConfigureNavigation(Func<Vector2> readNavigate)
        {
            _readNavigate = readNavigate;
        }

        private void Update()
        {
            if (_readNavigate == null)
                return;

            var current = CurrentSelectedIndex();
            var dir = current < 0 ? NavDir.None : Quantize(_readNavigate());
            if (dir == NavDir.None)
            {
                _lastNav = NavDir.None;
                return;
            }

            if (dir != _lastNav)
            {
                HandleNav(current, dir, fresh: true);
                _repeatTimer = _repeatDelay;
                _repeatInterval = dir is NavDir.Up or NavDir.Down ? _verticalRepeatStartInterval : _horizontalRepeatInterval;
            }
            else
            {
                _repeatTimer -= Time.unscaledDeltaTime;
                if (_repeatTimer <= 0f)
                {
                    HandleNav(current, dir, fresh: false);
                    // 上下のみ加速。左右は一定間隔。
                    if (dir is NavDir.Up or NavDir.Down)
                        _repeatInterval = Mathf.Max(_verticalRepeatMinInterval, _repeatInterval * _verticalRepeatAcceleration);
                    _repeatTimer = _repeatInterval;
                }
            }

            _lastNav = dir;
        }

        private NavDir Quantize(Vector2 v)
        {
            if (Mathf.Abs(v.x) < _inputDeadzone && Mathf.Abs(v.y) < _inputDeadzone)
                return NavDir.None;
            if (Mathf.Abs(v.y) >= Mathf.Abs(v.x))
                return v.y > 0f ? NavDir.Up : NavDir.Down;
            return v.x > 0f ? NavDir.Right : NavDir.Left;
        }

        private void HandleNav(int current, NavDir dir, bool fresh)
        {
            switch (dir)
            {
                case NavDir.Up:
                    StepVertical(current, -1, fresh);
                    break;
                case NavDir.Down:
                    StepVertical(current, 1, fresh);
                    break;
                case NavDir.Left:
                    StepHorizontal(current, -1);
                    break;
                case NavDir.Right:
                    StepHorizontal(current, 1);
                    break;
            }
        }

        // 上下: 端で停止。端にいる状態での新規入力時のみ反対端へループ。
        private void StepVertical(int current, int direction, bool fresh)
        {
            var target = Step(current, direction, 1);
            if (target != current)
            {
                Select(target);
                return;
            }
            if (!fresh)
                return;
            var wrap = direction < 0 ? LastInteractableIndex() : FirstInteractableIndex();
            if (wrap >= 0 && wrap != current)
                Select(wrap);
        }

        // 左右: 端でクランプのみ。ループ・加速なし。
        private void StepHorizontal(int current, int direction)
        {
            var target = Step(current, direction, _horizontalJump);
            if (target != current)
                Select(target);
        }

        // direction 方向へ、選択不能な項目はスキップしつつ最大 distance 個ぶん
        // 「選択可能な」項目を進み、到達した最後の選択可能項目を返す。
        // 端に達して進めない場合や、その方向に選択可能項目が無い場合は from を返す。
        private int Step(int from, int direction, int distance)
        {
            if (_itemViews.Count == 0)
                return from;
            var last = _itemViews.Count - 1;
            var result = from;
            var i = from;
            var moved = 0;
            while (moved < distance)
            {
                var next = i + direction;
                if (next < 0 || next > last)
                    break;
                i = next;
                if (IsSelectable(i))
                {
                    result = i;
                    moved++;
                }
            }
            return result;
        }

        private bool IsSelectable(int index) => !_canSkip || _itemViews[index].interactable;

        private int FirstInteractableIndex()
        {
            for (var i = 0; i < _itemViews.Count; i++)
                if (IsSelectable(i))
                    return i;
            return -1;
        }

        private int LastInteractableIndex()
        {
            for (var i = _itemViews.Count - 1; i >= 0; i--)
                if (IsSelectable(i))
                    return i;
            return -1;
        }

        private int CurrentSelectedIndex()
        {
            var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            if (selected == null)
                return -1;
            for (var i = 0; i < _itemViews.Count; i++)
                if (_itemViews[i] != null && _itemViews[i].gameObject == selected)
                    return i;
            return -1;
        }

        public ItemViewData GetItem(int index)
        {
            Log.Verbose($"[View]StorageView GetItem: {index}");
            return _itemViews[index].ItemData;
        }

        private int GetIndex(InventoryItemView itemView)
        {
            return _itemViews.IndexOf(itemView);
        }

        // 方向移動は自前の入力ループ(Update)が担うため、入力モジュールによる
        // Selectable のナビゲーション移動は無効化する。
        private void DisableSelectableNavigation()
        {
            foreach (var view in _itemViews)
            {
                var selectable = view.GetComponent<Selectable>();
                var nav = selectable.navigation;
                nav.mode = Navigation.Mode.None;
                selectable.navigation = nav;
            }
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

            DisableSelectableNavigation();
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

        private void UpdateSiblingOrder()
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
            DisableSelectableNavigation();
            UpdateSiblingOrder();
        }

        public void Remove(int index)
        {
            Log.Verbose($"[View]StorageView Remove: {index}");
            Destroy(ItemViews[index].gameObject);
            _itemViews.RemoveAt(index);
            DisableSelectableNavigation();
            UpdateSiblingOrder();
        }

        public void Replace(ItemViewData itemData, int index, bool interactable)
        {
            Log.Verbose($"[View]StorageView Replace: {index}");
            var view = _itemViews[index];
            view.Set(itemData);
            view.UpdateInteractable(interactable);
        }

        internal void UpdateItemInteractable(int index, bool interactable)
        {
            Log.Verbose($"[View]StorageView UpdateItemState: {index}");
            var view = _itemViews[index];
            view.UpdateInteractable(interactable);
        }

        internal void UpdateItemSkip(bool canSkip)
        {
            Log.Verbose($"[View]StorageView UpdateItemSkip: {canSkip}");
            _canSkip = canSkip;
        }
    }
}