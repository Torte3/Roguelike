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
using Domain.Model.WorldEvents;
using Domain.Service.Items;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace Domain.Service.Events
{
    public class MagicPot : IDisposable, ISerializable<MagicPotMemento>, IPlayerEventEntity
    {
        private int _remainingUsages;
        private bool CanUse => _remainingUsages > 0;
        public EntityBase Entity { get; init; }
        public bool IsGrounded => true;

        public MagicPot(MagicPotMemento data)
        {
            Entity = new EntityBase(data.Entity);
            _remainingUsages = data.RemainingUsages;
            Events = new List<IPlayerEvent>
            {
                new PlayerEvent(
                    FixtureKind.MagicPot,
                    new List<PlayerChoiceEvent>
                    {
                        new(
                            "使う",
                            (player, map) => CanUse,
                            async (_, map) => await DoEvent(map)
                        )
                    }
                )
            };
        }

        public void Dispose()
        {
            Entity.Dispose();
        }

        private Sprite Icon => Addressables.LoadAssetAsync<Sprite>($"Assets/Images/icons_full_16.png[icons_full_16_{(CanUse ? 270 : 269)}]")
            .WaitForCompletion();

        public IReadOnlyList<IPlayerEvent> Events { get; init; }

        private async UniTask DoEvent(IMap map)
        {
            var player = map.Player;
            var mergeBaseItemIndex = await player.Character.SelectItemWithCanSelect(
                "ベースのアイテムを選択してください",
                item => item.CanBeMergeBase);
            if (mergeBaseItemIndex == null)
                return;
            var mergeBaseItem = player.Character.Inventory.GetItem(mergeBaseItemIndex.Value);

            if (mergeBaseItem.ShouldRevealMimic(player.Character, player.Character.Entity.CurrentPosition, map))
            {
                return;
            }
            if (player.Character.Inventory.TakeOutCheck(mergeBaseItem).IsFailed(out var baseFailure))
            {
                map.Events.Record(new FacilityItemFailed(mergeBaseItem.NameIn(map), baseFailure));
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
                    return new ItemSelectPreview(new ItemFocus(0), mergeBaseItem.MergeWith(item), null);
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
            if (player.Character.Inventory.TakeOutCheck(mergedItem).IsFailed(out var mergedFailure))
            {
                map.Events.Record(new FacilityItemFailed(mergedItem.NameIn(map), mergedFailure));
                return;
            }
            if (mergedItem.PutInCheck().IsFailed(out var putInFailure))
            {
                map.Events.Record(new FacilityItemFailed(mergedItem.NameIn(map), putInFailure));
                return;
            }
            if (!player.Character.Inventory.CanAddIgnoreEmptySpace())
            {
                map.Events.Record(new MergedItemNotStored());
                return;
            }
            map.Events.Record(new FacilityUsed(FixtureKind.MagicPot));
            player.Character.Inventory.Replace(mergeBaseItem, mergeBaseItem.MergeWith(mergedItem));
            player.Character.Inventory.Remove(mergedItem);
            map.Events.Record(new ItemsMerged(player.Character.Label, mergeBaseItem.NameIn(map), mergedItem.NameIn(map),
                player.Character.InventoryLookIn(map), map.ShopLookIn()));
            ConsumeUse();
        }

        private void ConsumeUse()
        {
            _remainingUsages -= 1;
            if (!CanUse)
                Entity.Record(new FacilityExhausted(Entity.Ref, Icon));
        }

        public EntityLabel LabelIn(IMap map)
        {
            return new KindEntityLabel(FixtureKind.MagicPot);
        }

        public WorldEvent Appeared(IMap map)
        {
            return new FacilityAppeared(Entity.Ref, Entity.AppearanceOf(FixtureKind.MagicPot.ToEntityKind(), Icon), CanUse);
        }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            return UniTask.CompletedTask;
        }

        public MagicPotMemento Serialize()
        {
            return new MagicPotMemento(_remainingUsages, Entity.Serialize());
        }

        public static MagicPotMemento Build(Vector2Int position)
        {
            return new MagicPotMemento(3, EntityBase.Build(position, EntityLayer.Middle));
        }
    }
}