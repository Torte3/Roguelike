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

        public ReadOnlyReactiveProperty<bool> IsUsingKeyboard => _isUsingKeyboard;

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

        public Observable<Vector2> OnMovePerformed =>
            _actions.Field.Move.AsObservable().Select(context => context.ReadValue<Vector2>());

        public Vector2 MoveVector => _actions.Field.Move.ReadValue<Vector2>();

        public Vector2 NavigateVector => _actions.UI.Navigate.ReadValue<Vector2>();

        public Vector2 SelectItemVector => _actions.Field.SelectItem.ReadValue<Vector2>();

        public bool IsSelectItemModifier => _actions.Field.SelectItemModifier.IsPressed();

        public Vector2 InventoryNavigateVector
        {
            get
            {
                if (!_actions.Field.SelectItem.enabled)
                    return NavigateVector;
                return IsSelectItemModifier ? MoveVector : SelectItemVector;
            }
        }
        public ReadOnlyReactiveProperty<bool> IsDash => _actions.Field.Dash.AsEnabledPressedReactiveProperty();
        public ReadOnlyReactiveProperty<bool> IsNoMove => _actions.Field.TurnOnly.AsEnabledPressedReactiveProperty();
        public ReadOnlyReactiveProperty<bool> IsDiagonalOnly => _actions.Field.DiagonalOnly.AsEnabledPressedReactiveProperty();

        public bool IsDashPressed => IsPressed(_actions.Field.Dash);
        public bool IsNoMovePressed => IsPressed(_actions.Field.TurnOnly);
        public bool IsDiagonalOnlyPressed => IsPressed(_actions.Field.DiagonalOnly);

        public Observable<Unit> OnAttackPerformed =>
            _actions.Field.Attack.AsObservable().Select(context => Unit.Default);

        public Observable<Unit> OnSubmitPerformed =>
            _actions.UI.Submit.AsObservable().Select(_ => Unit.Default);

        public Observable<Unit> OnThrowPerformed => _actions.Field.Throw.AsObservable().Select(context => Unit.Default);
        public Observable<Unit> OnSwapItemPerformed => _actions.Field.SwapItem.AsObservable().Select(context => Unit.Default);

        public Observable<Unit> OnDoNothingPerformed =>
            _actions.Field.DoNothing.AsObservable().Select(context => Unit.Default);

        public bool IsDoNothingPerformed => _actions.Field.DoNothing.IsPressed();

        public Observable<Unit> OnRenamePerformed =>
            _actions.Field.Rename.AsObservable().Select(context => Unit.Default);

        public Observable<Unit> OnMainMenuOpening => _actions.Field.OpenMainMenu.AsObservable().Select(context => Unit.Default);
        public Observable<Unit> OnMenuCanceling => _actions.Menu.Cancel.AsObservable().Select(context => Unit.Default);
        public Observable<Unit> OnMenuClosing => _actions.Menu.Close.AsObservable().Select(context => Unit.Default);

        public void Dispose()
        {
            InputSystem.onActionChange -= OnActionChange;
            _isUsingKeyboard.Dispose();
            _disposables.Dispose();
        }

        private enum InputMode { Field, Menu }
        private InputMode _mode = InputMode.Field;

        public void Enable()
        {
            ApplyMode();
        }

        public void Disable()
        {
            _actions.Disable();
        }

        public void SwitchMenu()
        {
            Log.Info("[Input] Switch input to Menu");
            _mode = InputMode.Menu;
            ApplyMode();
        }

        public void SwitchField()
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
                _actions.UI.Disable();
            }
        }

        public void ApplyFaceButtonSwap(bool enabled)
        {
            ApplyFaceButtonSwapToAsset(_actions.asset, enabled);
        }

        private static bool IsPressed(InputAction action) => action.enabled && action.IsPressed();

        public void BindToUIModule()
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
