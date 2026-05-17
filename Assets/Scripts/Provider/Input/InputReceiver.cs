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
        public ReadOnlyReactiveProperty<bool> IsDash => _actions.Field.Dash.AsEnabledPressedReactiveProperty();
        public ReadOnlyReactiveProperty<bool> IsNoMove => _actions.Field.TurnOnly.AsEnabledPressedReactiveProperty();
        public ReadOnlyReactiveProperty<bool> IsDiagonalOnly => _actions.Field.DiagonalOnly.AsEnabledPressedReactiveProperty();

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
            _disposables.Dispose();
        }

        public void Enable()
        {
            _actions.Enable();
        }

        public void Disable()
        {
            _actions.Disable();
        }

        public void SwitchMenu()
        {
            Log.Info("[Input] Switch input to Menu");
            _actions.Menu.Enable();
            _actions.Field.Disable();
            _actions.UI.Enable();
        }

        public void SwitchField()
        {
            Log.Info("[Input] Switch input to Field");
            _actions.Field.Enable();
            _actions.Menu.Disable();
            _actions.UI.Disable();
        }

        public void ApplyFaceButtonSwap(bool enabled)
        {
            ApplyFaceButtonSwapToAsset(_actions.asset, enabled);

            var uiAsset = GetUIModuleActionsAssetOrNull();
            if (enabled && uiAsset == null)
                throw new InvalidOperationException(
                    "[Input] SwapABXY is enabled, but EventSystem/InputSystemUIInputModule (or its ActionsAsset) was not found.");
            if (uiAsset != null && !ReferenceEquals(uiAsset, _actions.asset))
                ApplyFaceButtonSwapToAsset(uiAsset, enabled);
        }

        private InputActionAsset? GetUIModuleActionsAssetOrNull()
        {
            var es = EventSystem.current;
            var uiModule = es?.GetComponent<InputSystemUIInputModule>();
            return uiModule?.actionsAsset;
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
