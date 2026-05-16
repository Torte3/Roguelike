using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Items;
using Domain.Service.Logs;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace Domain.Service.Events
{
    public class MagicPot : IDisposable, ISerializable<MagicPotMemento>, IPlayerEventEntity, IIconEntity
    {
        private ReactiveProperty<int> _remainingUsages;
        public ReadOnlyReactiveProperty<bool> CanUse => _remainingUsages.Select(remainingUsages => remainingUsages > 0).ToReadOnlyReactiveProperty();
        public EntityBase Entity { get; init; }
        public bool IsGrounded => true;

        public MagicPot(MagicPotMemento data)
        {
            Entity = new EntityBase(data.Entity);
            _remainingUsages = new ReactiveProperty<int>(data.RemainingUsages);
            Events = new List<IPlayerEvent>
            {
                new PlayerEvent(
                    "魔法の壺を見つけた",
                    new List<PlayerChoiceEvent>
                    {
                        new(
                            "使う",
                            (player, map) => CanUse.CurrentValue,
                            async (gameManager, map) => await DoEvent(gameManager, map)
                        )
                    }
                )
            };
        }

        public void Dispose()
        {
            Entity.Dispose();
        }

        public Sprite Icon => Addressables.LoadAssetAsync<Sprite>($"Assets/Images/icons_full_16.png[icons_full_16_{(CanUse.CurrentValue ? 270 : 269)}]")
            .WaitForCompletion();

        public IReadOnlyList<IPlayerEvent> Events { get; init; }

        private async UniTask DoEvent(IGameManager gameManager, IMap map)
        {
            var player = map.Player;
            var mergeBaseItemIndex = await player.Character.SelectItemWithCanSelect(
                "ベースのアイテムを選択してください",
                ItemMergeExtension.CanSelectForBaseItem);
            if (mergeBaseItemIndex == null)
                return;
            var mergeBaseItem = player.Character.Inventory.GetItem(mergeBaseItemIndex.Value);

            if (mergeBaseItem.ShouldRevealMimic(player.Character, player.Character.Entity.CurrentPosition, map))
            {
                return;
            }
            if (!player.Character.Inventory.CanRemove(mergeBaseItem))
            {
                GameLog.AddIgnoreVisibility($"{mergeBaseItem.GetName(player, map.ItemPlaceholders)}は取り出せなかった");
                return;
            }

            var mergedItemIndex = await player.Character.SelectItemWithCanSelectPreview(
                "合成するアイテムを選択してください",
                item => ItemMergeExtension.CanSelectForMergedItem(item, mergeBaseItem),
                item =>
                {
                    var canMerge = ItemMergeExtension.CanSelectForMergedItem(item, mergeBaseItem);
                    if (!canMerge)
                    {
                        return null;
                    }
                    return new ItemSelectPreview(new ItemFocus(0), mergeBaseItem.Merge(item), null);
                },
                new ItemSelectPreview(new ItemFocus(0), mergeBaseItem, "（合成されていません）\n"),
                "<b>合成結果...</b>");
            if (mergedItemIndex == null)
                return;
            var mergedItem = player.Character.Inventory.GetItem(mergedItemIndex.Value);

            if (mergedItem.ShouldRevealMimic(player.Character, player.Character.Entity.CurrentPosition, map))
            {
                return;
            }
            if (!player.Character.Inventory.CanRemove(mergedItem))
            {
                GameLog.AddIgnoreVisibility($"{mergedItem.GetName(player, map.ItemPlaceholders)}は取り出せなかった");
                return;
            }
            if (mergedItem.IsDiscardBlocked)
            {
                GameLog.AddIgnoreVisibility($"{mergedItem.GetName(player, map.ItemPlaceholders)}は呪われていて入れられない");
                return;
            }
            if (!player.Character.Inventory.CanAddIgnoreEmptySpace())
            {
                GameLog.AddIgnoreVisibility($"合成したアイテムがインベントリに入れられなかった");
                return;
            }
            player.Character.Inventory.Replace(mergeBaseItem, mergeBaseItem.Merge(mergedItem));
            player.Character.Inventory.Remove(mergedItem);
            GameLog.AddIgnoreVisibility($"{player.Character.GetName(player)}は{mergeBaseItem.GetName(player, map.ItemPlaceholders)}と{mergedItem.GetName(player, map.ItemPlaceholders)}を合成した。");
            gameManager.PlaySE(SE.MagicPotEnhance);
            _remainingUsages.Value -= 1;
        }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            return UniTask.CompletedTask;
        }

        public MagicPotMemento Serialize()
        {
            return new MagicPotMemento(_remainingUsages.CurrentValue, Entity.Serialize());
        }

        public static MagicPotMemento Build(Vector2Int position)
        {
            return new MagicPotMemento(3, EntityBase.Build(position, EntityLayer.Middle));
        }
    }
}