#nullable enable
using System.Linq;
using Configuration;
using Cysharp.Threading.Tasks;
using Domain.Service.Characters.Behavior;
using Domain.Service.Events;
using Game;
using IngameDebugConsole;
using Provider.Input;
using Provider.Texts;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using Utilities;
using VContainer;
using View;
using View.Playback;
using View.UI;

namespace Provider
{
    public class InputPresenter
    {
        [Inject]
        public InputPresenter(InputReceiver receiver, GameInput input, CharacterControlInputReceiver actionReceiver,
            ChoiceReceiver choiceReceiver, CharacterSelectReceiver characterSelectReceiver, TextInputReceiver textInputReceiver,
            MenuController menuController, InventoryView inventoryView, PlaybackQueue playback, DashState dash)
        {
            dash.IsDashing = () => receiver.IsDashPressed;
            actionReceiver.SetReadyWaiter(playback.WaitUntilIdle);

            var logWindowVisible = Observable.EveryValueChanged(DebugLogManager.Instance, x => x.IsLogWindowVisible).ToReadOnlyReactiveProperty();
            var textInputShown = new ReactiveProperty<bool>(false);
            Observable.CombineLatest(logWindowVisible, actionReceiver.IsEnabled.SkipLatestValueOnSubscribe(), textInputShown)
                .Subscribe(states =>
                {
                    var isInputBlocked = states[0] || states[2];
                    var isActionEnabled = states[1];

                    if (isInputBlocked || !isActionEnabled)
                        receiver.Disable();
                    else
                        receiver.Enable();
                });

            // L（SelectItemModifier）押下中は移動入力をアイテム選択に使うため、キャラ移動は抑制する。
            receiver.OnMovePerformed
                .Where(_ => !receiver.IsSelectItemModifier)
                .Select(vector => DirectionMethods.NearestDirectionFromVector(vector))
                .WhereNotNull()
                .Subscribe(direction => actionReceiver.SetMoveInput(direction, true));
            actionReceiver.OnActionRead
                .Where(_ => !receiver.IsSelectItemModifier)
                .Select(_ => receiver.MoveVector)
                .Select(vector => DirectionMethods.NearestDirectionFromVector(vector))
                .WhereNotNull()
                .Subscribe(direction => actionReceiver.SetMoveInput(direction, false));

            // フィールド中のアイテム選択（SelectItem）とメニューのナビゲーションを、インベントリのカーソル移動へ。
            inventoryView.ConfigureNavigation(() => receiver.InventoryNavigateVector);
            // フォーカスは InventoryView が単一所有。行動時に現在値を都度読む（コピー/ミラーを持たない）。
            actionReceiver.SetItemFocusProvider(() => inventoryView.CurrentFocus.ToItemFocus());
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
            receiver.OnMenuCanceling.Subscribe(_ =>
            {
                // アイテム選択中はキャンセルを Empty 選択として扱う（待受中でなければ無害）。
                actionReceiver.SetItemSelectCancelInput();
                menuController.CloseMenu();
            });
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
                await playback.WaitUntilIdle();
                var choices = message.choices
                    .Select(choice => (choice.choice, ToneText.Of(choice.infoTitle), choice.info))
                    .ToArray();
                var index = await menuController.GetChoiceWithInfo(ToneText.Of(message.message), message.defaultIndex, message.clearPreviousMenus, choices);
                choiceReceiver.SetChoicedIndex(index);
            });

            choiceReceiver.OnShownChoice.Subscribe(async message =>
            {
                await playback.WaitUntilIdle();
                var index = message.cancelChoiceIndex is { } cancelIndex
                    ? await menuController.GetChoice(ToneText.Of(message.message), cancelIndex, message.choices)
                    : await menuController.GetChoice(ToneText.Of(message.message), message.choices);
                choiceReceiver.SetChoicedIndex(index);
            });

            characterSelectReceiver.OnShownChoice.Subscribe(async message =>
            {
                await playback.WaitUntilIdle();
                var index = await menuController.GetCharacter(message
                    .Select(character => (character.name, character.textureName,
                        UnlockText.Of(character.unlock, character.info), character.unlock.IsUnlocked))
                    .ToList());
                characterSelectReceiver.SetChoicedIndex(index);
            });

            textInputReceiver.OnShownTextInput.Subscribe(async canCancel =>
            {
                await playback.WaitUntilIdle();
                textInputShown.Value = true;
                var text = await menuController.GetTextInput(canCancel);
                textInputReceiver.SetTextInput(text);
                textInputShown.Value = false;
            });
        }

        private async UniTaskVoid ApplySwapAfterEventSystemInitialized(InputReceiver receiver)
        {
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

            // UIモジュールを InputReceiver と同一アセットへ束ねてから Swap を適用する。
            receiver.BindToUIModule();

            Settings.GlobalSettings.SwapABXY.Value
                .SubscribeIncludingCurrentValue(receiver.ApplyFaceButtonSwap);
        }
    }
}