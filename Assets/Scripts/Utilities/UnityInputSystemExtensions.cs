using System;
using R3;
using UnityEngine.InputSystem;

namespace Utilities
{
    public static class UnityInputSystemExtensions
    {
        public static Observable<InputAction.CallbackContext> AsObservable(this InputAction action)
        {
            return Observable.FromEvent<InputAction.CallbackContext>(
                h => action.performed += h,
                h => action.performed -= h);
        }

        private static Observable<InputAction.CallbackContext> AsPerformedOrCanceledObservable(InputAction action)
        {
            return Observable.FromEvent<InputAction.CallbackContext>(
                h =>
                {
                    action.performed += h;
                    action.canceled += h;
                },
                h =>
                {
                    action.performed -= h;
                    action.canceled -= h;
                });
        }

        public static ReadOnlyReactiveProperty<T> AsReactiveProperty<T>(this InputAction action) where T : struct
        {
            return AsPerformedOrCanceledObservable(action)
                .Select(context => context.ReadValue<T>())
                .ToReadOnlyReactiveProperty();
        }

        public static ReadOnlyReactiveProperty<bool> AsPressedReactiveProperty(this InputAction action)
        {
            return AsPerformedOrCanceledObservable(action)
                .Select(_ => action.IsPressed())
                .ToReadOnlyReactiveProperty();
        }

        /// <summary>
        /// 「enabledでなければ必ずfalse」を保証する押下状態ReactivePropertyを返す。
        /// enabledの変化時も即座に再評価される。
        /// </summary>
        public static ReadOnlyReactiveProperty<bool> AsEnabledPressedReactiveProperty(this InputAction action)
        {
            var pressOrRelease = AsPerformedOrCanceledObservable(action).AsUnitObservable();
            var enabledOrDisabled = AsEnabledChangeObservable(action);

            return Observable.Merge(pressOrRelease, enabledOrDisabled)
                .Select(_ => action.enabled && action.IsPressed())
                .ToReadOnlyReactiveProperty();
        }

        private static Observable<Unit> AsEnabledChangeObservable(InputAction action)
        {
            Action<object, InputActionChange>? wrapped = null;

            return Observable.FromEvent<Unit>(
                h =>
                {
                    var map = action.actionMap;
                    wrapped = (changed, change) =>
                    {
                        switch (change)
                        {
                            case InputActionChange.ActionEnabled:
                            case InputActionChange.ActionDisabled:
                                if (ReferenceEquals(changed, action))
                                    h(Unit.Default);
                                break;
                            case InputActionChange.ActionMapEnabled:
                            case InputActionChange.ActionMapDisabled:
                                if (map != null && ReferenceEquals(changed, map))
                                    h(Unit.Default);
                                break;
                        }
                    };

                    InputSystem.onActionChange += wrapped;
                },
                _ =>
                {
                    if (wrapped == null)
                        return;
                    InputSystem.onActionChange -= wrapped;
                    wrapped = null;
                });
        }
    }
}