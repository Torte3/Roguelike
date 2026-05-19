#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model.Setting;
using Domain.Service.Characters.Behavior;
using Domain.Service.Events;
using Game;
using IngameDebugConsole;
using Provider.Input;
using R3;
using UnityEngine;
using Utilities;
using VContainer;
using View;
using View.UI;

namespace Provider
{
    public class InputPresenter
    {
        [Inject]
        public InputPresenter(InputReceiver receiver, GameInput input, CharacterControlInputReceiver actionReceiver,
            ChoiceReceiver choiceReceiver, CharacterSelectReceiver characterSelectReceiver, TextInputReceiver textInputReceiver, World world,
            MenuController menuController, InventoryView inventoryView)
        {
            var logWindowVisible = Observable.EveryValueChanged(DebugLogManager.Instance, x => x.IsLogWindowVisible).ToReadOnlyReactiveProperty();
            var textInputShown = new ReactiveProperty<bool>(false);
            Observable.CombineLatest(logWindowVisible, actionReceiver.IsEnabled, textInputShown)
                .Subscribe(x =>
                {
                    if (x[0] || !x[1] || x[2])
                        receiver.Disable();
                    else
                        receiver.Enable();
                });

            receiver.OnMovePerformed
                .Select(vector => DirectionMethods.NearestDirectionFromVector(vector))
                .WhereNotNull()
                .Subscribe(direction => actionReceiver.SetMoveInput(direction, true));
            actionReceiver.OnActionRead
                .Select(_ => receiver.MoveVector)
                .Select(vector => DirectionMethods.NearestDirectionFromVector(vector))
                .WhereNotNull()
                .Subscribe(direction => actionReceiver.SetMoveInput(direction, false));

            inventoryView.Focus.Subscribe(focus =>
                actionReceiver.SetItemFocus(focus.ToItemFocus()));
            receiver.OnAttackPerformed.Subscribe(_ => actionReceiver.SetAttackInput());
            receiver.OnSubmitPerformed
                .Subscribe(_ => actionReceiver.SetItemSelectConfirmInput());
            receiver.OnThrowPerformed.Subscribe(_ => actionReceiver.SetThrowInput());
            receiver.OnSwapItemPerformed.Subscribe(_ => actionReceiver.SetDropInput());
            receiver.OnDoNothingPerformed.Subscribe(_ => actionReceiver.SetDoNothingInput());
            actionReceiver.OnActionRead
                .Where(_ => receiver.IsDoNothingPerformed)
                .Subscribe(_ => actionReceiver.SetDoNothingInput());
            receiver.OnRenamePerformed.Subscribe(_ => actionReceiver.SetRenameInput());

            input.Bind(
                () => receiver.IsDashPressed,
                () => receiver.IsNoMovePressed,
                () => receiver.IsDiagonalOnlyPressed);
            
            receiver.IsNoMove
                .DistinctUntilChanged()
                .Where(isNoMove => isNoMove)
                .Where(_ => receiver.MoveVector == Vector2.zero)
                .Subscribe(_ => actionReceiver.SetFaceNearestCharacterInput());

            receiver.OnMainMenuOpening.Subscribe(_ => menuController.OpenMeinMenu());
            receiver.OnMenuCanceling.Subscribe(_ => menuController.CloseMenu());
            receiver.OnMenuClosing.Subscribe(_ => menuController.CloseAllMenus());

            ApplySwapAfterEventSystemInitialized(receiver);
    
            menuController.MenuState.Subscribe(menuState =>
            {
                switch (menuState)
                {
                    case MenuType.Field:
                        receiver.SwitchField();
                        break;
                    case MenuType.Menu:
                        receiver.SwitchMenu();
                        break;
                }
            });

            choiceReceiver.OnShownChoiceWithInfo.Subscribe(async message =>
            {
                var index = message.cancelChoiceIndex is { } cancelIndex
                    ? await menuController.GetChoiceWithInfo(message.text, cancelIndex, message.choices)
                    : await menuController.GetChoiceWithInfo(message.text, message.choices);
                choiceReceiver.SetChoicedIndex(index);
            });

            choiceReceiver.OnShownChoice.Subscribe(async message =>
            {
                var index = message.cancelChoiceIndex is { } cancelIndex
                    ? await menuController.GetChoice(message.text, cancelIndex, message.choices)
                    : await menuController.GetChoice(message.text, message.choices);
                choiceReceiver.SetChoicedIndex(index);
            });

            characterSelectReceiver.OnShownChoice.Subscribe(async message =>
            {
                var index = await menuController.GetCharacter(message);
                characterSelectReceiver.SetChoicedIndex(index);
            });

            textInputReceiver.OnShownTextInput.Subscribe(async canCancel =>
            {
                textInputShown.Value = true;
                var text = await menuController.GetTextInput(canCancel);
                textInputReceiver.SetTextInput(text);
                textInputShown.Value = false;
            });
        }

        private async UniTaskVoid ApplySwapAfterEventSystemInitialized(InputReceiver receiver)
        {
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

            Settings.GlobalSettings.SwapABXY.Value
                .SubscribeIncludingCurrentValue(receiver.ApplyFaceButtonSwap);
        }
    }
}