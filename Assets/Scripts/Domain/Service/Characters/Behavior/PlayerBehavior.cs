#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Configuration;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Dungeon;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.WorldEvents;
using Domain.Service.Action;
using Domain.Service.Events;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Service.Characters.Behavior
{
    internal sealed class PlayerBehavior : ICharacterBehavior
    {
        private readonly IntelligentDashController _intelligentDashController = new();
        private readonly CharacterControlInputReceiver _receiver;
        private readonly IGameManager _gameManager;
        public BehaviorData BehaviorData => new();
        private readonly Subject<OnStartItemSelectMessage> _onStartItemSelect = new();
        public Observable<OnStartItemSelectMessage> OnStartItemSelect => _onStartItemSelect;
        private readonly Subject<Unit> _onSelectedItemSelect = new();
        public Observable<Unit> OnSelectedItemSelect => _onSelectedItemSelect;
        private Option<Location> _homeLocation;

        private enum InputType
        {
            Move,
            FaceNearestCharacter,
            UseItem,
            ThrowItem,
            SwapItem,
            DoNothing,
            RenameItem
        }

        public PlayerBehavior(CharacterControlInputReceiver receiver, IGameManager gameManager)
        {
            _receiver = receiver;
            _gameManager = gameManager;
        }

        public BehaviorMemento Serialize()
        {
            return new BehaviorMemento(BehaviorData, _homeLocation, Option<BehaviorState>.None, Option<Location>.None);
        }

        public static BehaviorMemento Build()
        {
            return new BehaviorMemento(new BehaviorData(), Option<Location>.None, Option<BehaviorState>.None, Option<Location>.None);
        }

        public bool WanderAround => true;

        public async UniTask<IAction> GenerateNextAction(IHasBehavior character, IGameManager gameManager, IMap map,
            IInput input)
        {
            Log.Debug("[Think] Start waiting input...");
            await _receiver.WaitUntilReady();
            if (input.IsDash()) await _intelligentDashController.Wait(character, map);

            var tasks = InitializeTasks();
            _receiver.ReadInput();
            try
            {
                return await ReadAction(character, gameManager, map, input, await tasks);
            }
            finally
            {
                _receiver.FinishReading();
            }
        }

        private async UniTask<IAction> ReadAction(IHasBehavior character, IGameManager gameManager, IMap map,
            IInput input, (InputType type, (Move action, bool isStarted)? move, ItemFocus? focus) result)
        {
            // 入力種別ごとの処理に振り分ける。各ハンドラは「確定した行動」を返し、null の場合は
            // まだ行動が確定していないので、次の入力を待ち直してループを続ける。
            while (true)
            {
                switch (result.type)
                {
                    case InputType.Move:
                    {
                        var action = await HandleMoveInput(character, gameManager, map, input, result.move!.Value);
                        if (action != null)
                            return action;
                        break;
                    }
                    case InputType.FaceNearestCharacter:
                        character.FaceNearestCharacter(map);
                        break;
                    case InputType.UseItem:
                    {
                        var action = HandleUseItemInput(character, map, result.focus!);
                        if (action != null)
                            return action;
                        break;
                    }
                    case InputType.ThrowItem:
                    {
                        var action = HandleThrowItemInput(character, map, result.focus!);
                        if (action != null)
                            return action;
                        break;
                    }
                    case InputType.SwapItem:
                    {
                        var action = await HandleSwapItemInput(character, map, result.focus!);
                        if (action != null)
                            return action;
                        break;
                    }
                    case InputType.DoNothing:
                        await UniTask.Yield();
                        return new DoNothing();
                    case InputType.RenameItem:
                        await HandleRenameItemInput(gameManager, map, character, result.focus!);
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }

                result = await InitializeTasks();
            }
        }

        // 移動入力の処理。確定した行動（移動・入れ替え・イベント・何もしない）を返す。確定しなければ null。
        private async UniTask<IAction?> HandleMoveInput(IHasBehavior character, IGameManager gameManager, IMap map,
            IInput input, (Move action, bool isStarted) moveInput)
        {
            var (move, started) = moveInput;
            var destination = character.Entity.CurrentPosition + move.Direction.Vector();
            var playerEventEntity = map
                .GetPlayerEventEntitiesFastAt(destination, EntityLayer.Middle, EntityLayer.Floor, EntityLayer.Bottom)
                .FirstOrDefault();

            // 移動を伴わない入力（向き変更のみ・斜め指定だが斜めでない・移動不可状態）。
            if (input.IsNoMove() ||
                (input.IsDiagonalOnly() && !move.Direction.IsDiagonal()) ||
                character.Status.IsFlagStat(FlagStatType.CannotMove))
            {
                character.Turn(move.Direction);
                var (eventAction, _) = await TryGetPlayerEventAction(character, gameManager, map, playerEventEntity, new Swap(move.Direction));
                if (eventAction != null)
                    return eventAction;
                if (character.Status.IsFlagStat(FlagStatType.CannotMove))
                    return new DoNothing();
            }
            else
            {
                if (Settings.GlobalSettings.IntelligentDash.CurrentValue)
                    move = _intelligentDashController.Filter(move, character, started, map, input);
                var swap = new Swap(move.Direction);

                character.Turn(move.Direction);
                if (move.Doable(character, map))
                    return move;
                var (eventAction, anyEventCanExecute) = await TryGetPlayerEventAction(character, gameManager, map, playerEventEntity, swap);
                if (eventAction != null)
                    return eventAction;
                if (!anyEventCanExecute && swap.Doable(character, map))
                    return swap;
            }

            return null;
        }

        // アイテム使用入力の処理。選択が空ならスキル、アイテムがあればそのアイテムを使う。実行可能なら返す。
        private IAction? HandleUseItemInput(IHasBehavior character, IMap map, ItemFocus focus)
        {
            var focusItem = focus.GetItem(character.Inventory, map);
            IAction action;
            if (focusItem == null)
                action = new UseSkill(character.Skills[0].Skill, character.CurrentDirection);
            else
                action = new UseItem(focusItem, character.CurrentDirection);

            return action.Doable(character, map) ? action : null;
        }

        // アイテム投擲入力の処理。投げられないアイテムなら null。呪いで投げられない場合はログを出す。
        private IAction? HandleThrowItemInput(IHasBehavior character, IMap map, ItemFocus focus)
        {
            if (focus.IsOnItem(character.Inventory, map, out var focusItem))
            {
                var action = new ThrowItem(focusItem, character.CurrentDirection);
                if (action.Doable(character, map))
                    return action;
                LogIfCursedBlocksThrowAfterDoableFailed(character, map, focusItem);
            }

            return null;
        }

        // 入れ替え入力の処理。拾う・落とす・インベントリ内入れ替えを判定する。確定すれば行動を返す。
        private async UniTask<IAction?> HandleSwapItemInput(IHasBehavior character, IMap map, ItemFocus focus)
        {
            if (focus.IsOnEmpty)
                return null;
            if (focus.IsOnGroundItem)
            {
                var pickUp = new PickUpItem();
                if (pickUp.Doable(character, map))
                    return pickUp;
            }

            var focus2 = await SelectItem("入れ替え先を選択してください", new ItemFocus[] { focus });
            if (focus2.IsOnEmpty)
                return null;

            var item1 = focus.GetItem(character.Inventory, map);
            var item2 = focus2.GetItem(character.Inventory, map);
            if (focus.IsOnGroundItem)
            {
                var drop = new DropItem(item2);
                if (drop.Doable(character, map))
                    return drop;
                LogIfCursedBlocksDropAfterDoableFailed(character, map, item2);
            }
            else if (focus2.IsOnGroundItem)
            {
                var drop = new DropItem(item1);
                if (drop.Doable(character, map))
                    return drop;
                LogIfCursedBlocksDropAfterDoableFailed(character, map, item1);
            }
            else
            {
                if (!character.Inventory.CanSwap(focus.Index, focus2.Index))
                    throw new Exception($"Can't swap item from inventory: focus: {focus}, focus2: {focus2}");
                character.Inventory.Swap(focus.Index, focus2.Index);
                map.Events.Record(new ItemsReordered(character.Entity.Ref, focus.Index, focus2.Index, character.InventoryLookIn(map)));
            }

            return new DoNothing();
        }

        // アイテム名変更入力の処理。種類への命名・個体への命名/初期化を、選択肢に応じて行う。
        private async UniTask HandleRenameItemInput(IGameManager gameManager, IMap map, IHasBehavior character,
            ItemFocus focus)
        {
            var focusItem = focus.GetItem(character.Inventory, map);
            if (focusItem == null)
                return;

            var choices = new List<string>();
            if (!map.Player.Character.IsKnownItem(focusItem))
            {
                choices.Add("このアイテムの種類に名前をつける");
            }
            if (focusItem.CustomName.IsSome())
            {
                choices.Add("このアイテム単体の名前を変える");
                choices.Add("このアイテム単体の名前をデフォルトに戻す");
            }
            else
            {
                choices.Add("このアイテム単体に名前をつける");
            }
            var cancelChoiceIndex = choices.Count;
            choices.Add("やめる");

            var choiceIndex = await gameManager.GetChoice(null, cancelChoiceIndex, choices.ToArray());
            if (choiceIndex == cancelChoiceIndex)
                return;

            switch (choices[choiceIndex])
            {
                case "このアイテムの種類に名前をつける":
                {
                    var typeName = await gameManager.GetTextInput(canCancel: true);
                    if (typeName != null)
                        map.ItemPlaceholders.Rename(focusItem.BaseName, typeName);
                    break;
                }
                case "このアイテム単体に名前をつける":
                {
                    var itemName = await gameManager.GetTextInput(canCancel: true);
                    if (itemName != null)
                        focusItem.Rename(itemName);
                    break;
                }
                case "このアイテム単体の名前をデフォルトに戻す":
                    focusItem.RevertToDefaultName();
                    break;
            }

            map.Events.Record(new ItemRenamed(character.Entity.Ref, character.WholeInventoryLookIn(map), map.PlayerUnderfoot()));
        }

        private static async UniTask<(IAction? action, bool anyEventCanExecute)> TryGetPlayerEventAction(IHasBehavior character, IGameManager gameManager, IMap map, IHasPlayerEvent? playerEventEntity, Swap swap)
        {
            if (playerEventEntity == null || !playerEventEntity.Events.Any(e => e.CanExecuteEvent(map.Player, map)))
                return (null, false);
            var eventAction = await playerEventEntity.Events.DoAction(map.Player, gameManager, map, swap);
            if (eventAction != null && eventAction.Doable(character, map))
                return (eventAction, true);
            return (null, true);
        }

        private async UniTask<(InputType type, (Move action, bool isStarted)? move, ItemFocus? focus)> InitializeTasks()
        {
            var cancellationToken = new CancellationTokenSource();
            UniTask<(Move action, bool isStarted)> moveTask =
                _receiver.OnMoveInputReceived.WaitAsync(cancellationToken.Token);
            var faceNearestTask = _receiver.OnFaceNearestCharacterActionReceived.WaitAsync(cancellationToken.Token);
            var useItemTask = _receiver.OnUseItemActionReceived.WaitAsync(cancellationToken.Token);
            var throwItemTask = _receiver.OnThrowItemActionReceived.WaitAsync(cancellationToken.Token);
            var swapItemTask = _receiver.OnSwapItemActionReceived.WaitAsync(cancellationToken.Token);
            var doNothingTask = _receiver.OnDoNothingActionReceived.WaitAsync(cancellationToken.Token);
            var renameItemTask = _receiver.OnRenameItemActionReceived.WaitAsync(cancellationToken.Token);

            var tasks = await UniTask.WhenAny(moveTask, faceNearestTask, useItemTask, throwItemTask, swapItemTask, doNothingTask,
                renameItemTask);
            cancellationToken.Cancel();
            return tasks.winArgumentIndex switch
            {
                0 => (InputType.Move, tasks.result1, null),
                1 => (InputType.FaceNearestCharacter, null, null),
                2 => (InputType.UseItem, null, tasks.result3),
                3 => (InputType.ThrowItem, null, tasks.result4),
                4 => (InputType.SwapItem, null, tasks.result5),
                5 => (InputType.DoNothing, null, null),
                6 => (InputType.RenameItem, null, tasks.result7),
                _ => throw new IndexOutOfRangeException()
            };
        }

        public void KnowLocationOf(Location location)
        {
        }

        public bool AcceptsSwapFrom(IHasBehavior character, Vector2Int requesterPosition, IMap map)
        {
            return false;
        }

        public async UniTask<ItemFocus> SelectItem(string text, params ItemFocus[] disabledItemIndexes)
        {
            await _receiver.WaitUntilReady();
            _onStartItemSelect.OnNext(new OnStartItemSelectMessage(text, disabledItemIndexes));
            var focus = await WaitItemSelectOrCancel(disabledItemIndexes);

            // 確定は Submit 入力の配信中（同期継続）に到達する。配信中に OnNext を発火すると、購読側の
            // 入力マップ切替が配信中のアクションを壊して IndexOutOfRange になるため、配信後（フレーム末）に発火する。
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            _onSelectedItemSelect.OnNext(Unit.Default);
            return focus;
        }

        public async UniTask<ItemFocus> SelectItemWithPreview(
            string text,
            ItemFocus[] disabledItemIndexes,
            ItemSelectPreview[] previews,
            ItemSelectPreview? defaultPreview,
            string previewTitle)
        {
            await _receiver.WaitUntilReady();
            _onStartItemSelect.OnNext(new OnStartItemSelectMessage(text, disabledItemIndexes, previews, defaultPreview, previewTitle));

            var focus = await WaitItemSelectOrCancel(disabledItemIndexes);

            // 確定は Submit 入力の配信中（同期継続）に到達する。配信中に OnNext を発火すると、購読側の
            // 入力マップ切替が配信中のアクションを壊して IndexOutOfRange になるため、配信後（フレーム末）に発火する。
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            _onSelectedItemSelect.OnNext(Unit.Default);
            return focus;
        }

        // アイテム選択の確定を待つ。メニューのキャンセルが押された場合は Empty 選択として扱う。
        private async UniTask<ItemFocus> WaitItemSelectOrCancel(ItemFocus[] disabledItemIndexes)
        {
            while (true)
            {
                var cts = new CancellationTokenSource();
                var (winIndex, confirmed, _) = await UniTask.WhenAny(
                    _receiver.OnItemSelectConfirmReceived.WaitAsync(cts.Token),
                    _receiver.OnItemSelectCancelReceived.WaitAsync(cts.Token));
                cts.Cancel();

                if (winIndex == 1)
                    return ItemFocus.Empty;

                if (confirmed.IsOnEmpty || !disabledItemIndexes.Contains(confirmed))
                    return confirmed;
            }
        }

        private static void LogIfCursedBlocksThrowAfterDoableFailed(IHasBehavior character, IMap map, IItem item)
        {
            if (character.Status.IsFlagStat(FlagStatType.CannotAct))
                return;
            if (!IsItemAccessibleForThrow(character, map, item))
                return;
            if (!item.ThrowCheck().IsFailed(out var failure))
                return;
            map.Events.Record(new ItemActionFailed(character.Entity.IsVisible, item.NameIn(map), failure));
        }

        private static void LogIfCursedBlocksDropAfterDoableFailed(IHasBehavior character, IMap map, IItem? item)
        {
            if (item == null || character.Status.IsFlagStat(FlagStatType.CannotAct))
                return;
            if (!character.Inventory.CanRemove(item))
                return;
            if (!item.DiscardCheck().IsFailed(out var failure))
                return;
            map.Events.Record(new ItemActionFailed(character.Entity.IsVisible, item.NameIn(map), failure));
        }

        private static bool IsItemAccessibleForThrow(IHasBehavior character, IMap map, IItem item) =>
            character.Inventory.CanRemove(item)
            || map.Items.At(character.Entity.CurrentPosition).FirstOrDefault()?.Item == item;
    }
}