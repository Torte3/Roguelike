#nullable enable
using System;
using System.Collections.Generic;
using R3;
using Unity.Logging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using Utilities;

namespace Provider.Input
{
    public class InputReceiver : IDisposable
    {
        private readonly MyInputAction _actions = new();
        private readonly CompositeDisposable _disposables = new();
        private readonly ReactiveProperty<bool> _isUsingKeyboard = new(Gamepad.current == null);

        /// <summary>直近に操作されたデバイスがキーボード/マウスなら true、ゲームパッドなら false。</summary>
        internal ReadOnlyReactiveProperty<bool> IsUsingKeyboard => _isUsingKeyboard;

        public InputReceiver()
        {
            InputSystem.onActionChange += OnActionChange;
        }

        private void OnActionChange(object obj, InputActionChange change)
        {
            if (change != InputActionChange.ActionPerformed || obj is not InputAction action)
                return;
            var device = action.activeControl?.device;
            if (device is Keyboard or Mouse)
                _isUsingKeyboard.Value = true;
            else if (device is Gamepad)
                _isUsingKeyboard.Value = false;
        }

        private static readonly Dictionary<string, string> FaceButtonSwapMap = new()
        {
            // XInputController (主にWindows/Xbox系)
            { "<XInputController>/buttonSouth", "<XInputController>/buttonEast" },
            { "<XInputController>/buttonEast", "<XInputController>/buttonSouth" },
            { "<XInputController>/buttonWest", "<XInputController>/buttonNorth" },
            { "<XInputController>/buttonNorth", "<XInputController>/buttonWest" },

            // Gamepad (Menu等で使われている)
            { "<Gamepad>/buttonSouth", "<Gamepad>/buttonEast" },
            { "<Gamepad>/buttonEast", "<Gamepad>/buttonSouth" },
            { "<Gamepad>/buttonWest", "<Gamepad>/buttonNorth" },
            { "<Gamepad>/buttonNorth", "<Gamepad>/buttonWest" },
        };

        internal Observable<Vector2> OnMovePerformed =>
            _actions.Field.Move.AsObservable().Select(context => context.ReadValue<Vector2>());

        internal Vector2 MoveVector => _actions.Field.Move.ReadValue<Vector2>();

        // UIモジュールは BindToUIModule で _actions と同一アセットを使うため、直接読める。
        private Vector2 NavigateVector => _actions.UI.Navigate.ReadValue<Vector2>();

        // フィールド中のアイテム選択入力（Field マップの SelectItem アクション：右スティック/矢印など）。
        private Vector2 SelectItemVector => _actions.Field.SelectItem.ReadValue<Vector2>();

        // アイテム選択修飾（L）。Field マップの SelectItemModifier アクション。
        // 押下中は移動入力をインベントリのカーソル移動に転用する（L+移動でアイテム選択）。
        internal bool IsSelectItemModifier => _actions.Field.SelectItemModifier.IsPressed();

        // インベントリのカーソル移動に使う入力。
        // メニュー等（Field 無効）では UI ナビゲーション。
        // フィールドでは、L 押下中は移動入力を、そうでなければ SelectItem（右スティック/矢印）をカーソルに使う。
        internal Vector2 InventoryNavigateVector
        {
            get
            {
                if (!_actions.Field.SelectItem.enabled)
                    return NavigateVector;
                return IsSelectItemModifier ? MoveVector : SelectItemVector;
            }
        }
        internal ReadOnlyReactiveProperty<bool> IsNoMove => _actions.Field.TurnOnly.AsEnabledPressedReactiveProperty();

        internal bool IsDashPressed => IsPressed(_actions.Field.Dash);
        internal bool IsNoMovePressed => IsPressed(_actions.Field.TurnOnly);
        internal bool IsDiagonalOnlyPressed => IsPressed(_actions.Field.DiagonalOnly);

        internal Observable<Unit> OnAttackPerformed =>
            _actions.Field.Attack.AsObservable().Select(context => Unit.Default);

        internal Observable<Unit> OnSubmitPerformed =>
            _actions.UI.Submit.AsObservable().Select(_ => Unit.Default);

        internal Observable<Unit> OnThrowPerformed => _actions.Field.Throw.AsObservable().Select(context => Unit.Default);
        internal Observable<Unit> OnSwapItemPerformed => _actions.Field.SwapItem.AsObservable().Select(context => Unit.Default);

        internal Observable<Unit> OnDoNothingPerformed =>
            _actions.Field.DoNothing.AsObservable().Select(context => Unit.Default);

        internal bool IsDoNothingPerformed => _actions.Field.DoNothing.IsPressed();

        internal Observable<Unit> OnRenamePerformed =>
            _actions.Field.Rename.AsObservable().Select(context => Unit.Default);

        internal Observable<Unit> OnMainMenuOpening => _actions.Field.OpenMainMenu.AsObservable().Select(context => Unit.Default);
        internal Observable<Unit> OnMenuCanceling => _actions.Menu.Cancel.AsObservable().Select(context => Unit.Default);
        internal Observable<Unit> OnMenuClosing => _actions.Menu.Close.AsObservable().Select(context => Unit.Default);

        public void Dispose()
        {
            InputSystem.onActionChange -= OnActionChange;
            _isUsingKeyboard.Dispose();
            _disposables.Dispose();
        }

        private enum InputMode { Field, Menu }
        private InputMode _mode = InputMode.Field;

        internal void Enable()
        {
            // 全マップを無条件に有効化すると、フィールドでも UI が有効になり
            // UI.Navigate(矢印) が SelectItem(矢印) と競合する。現在モードを復元する。
            ApplyMode();
        }

        internal void Disable()
        {
            _actions.Disable();
        }

        internal void SwitchMenu()
        {
            Log.Info("[Input] Switch input to Menu");
            _mode = InputMode.Menu;
            ApplyMode();
        }

        internal void SwitchField()
        {
            Log.Info("[Input] Switch input to Field");
            _mode = InputMode.Field;
            ApplyMode();
        }

        private void ApplyMode()
        {
            if (_mode == InputMode.Menu)
            {
                _actions.Menu.Enable();
                _actions.Field.Disable();
                _actions.UI.Enable();
            }
            else
            {
                _actions.Field.Enable();
                _actions.Menu.Disable();
                // フィールドでは UI を無効化（UI.Navigate の矢印が SelectItem と競合しないように）。
                _actions.UI.Disable();
            }
        }

        internal void ApplyFaceButtonSwap(bool enabled)
        {
            // UIモジュールも同一アセット（BindToUIModule）なので、このアセットへの適用だけで足りる。
            ApplyFaceButtonSwapToAsset(_actions.asset, enabled);
        }

        private static bool IsPressed(InputAction action) => action.enabled && action.IsPressed();

        // EventSystem の UIモジュールを、この InputReceiver と同一のアクションアセットへ束ねる。
        // これで「ゲームプレイ用」と「UIモジュール用」が1インスタンスに統一され、有効/無効や
        // SwapABXY の二重管理、NavigateVector の特殊対応が不要になる。
        internal void BindToUIModule()
        {
            var uiModule = EventSystem.current != null
                ? EventSystem.current.GetComponent<InputSystemUIInputModule>()
                : null;
            if (uiModule == null)
                return;

            uiModule.actionsAsset = _actions.asset;
            uiModule.move = InputActionReference.Create(_actions.UI.Navigate);
            uiModule.submit = InputActionReference.Create(_actions.UI.Submit);
            uiModule.cancel = InputActionReference.Create(_actions.UI.Cancel);
            uiModule.point = InputActionReference.Create(_actions.UI.Point);
            uiModule.leftClick = InputActionReference.Create(_actions.UI.Click);
            uiModule.middleClick = InputActionReference.Create(_actions.UI.MiddleClick);
            uiModule.rightClick = InputActionReference.Create(_actions.UI.RightClick);
            uiModule.scrollWheel = InputActionReference.Create(_actions.UI.ScrollWheel);
            uiModule.trackedDevicePosition = InputActionReference.Create(_actions.UI.TrackedDevicePosition);
            uiModule.trackedDeviceOrientation = InputActionReference.Create(_actions.UI.TrackedDeviceOrientation);
        }

        private void ApplyFaceButtonSwapToAsset(InputActionAsset asset, bool enabled)
        {
            foreach (var action in asset)
            {
                for (var i = 0; i < action.bindings.Count; i++)
                {
                    var binding = action.bindings[i];
                    if (!FaceButtonSwapMap.TryGetValue(binding.path, out var swappedPath))
                        continue;

                    if (enabled)
                        action.ApplyBindingOverride(i, new InputBinding { overridePath = swappedPath });
                    else
                        action.RemoveBindingOverride(i);
                }
            }
        }
    }
}
