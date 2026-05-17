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
using Utilities;

namespace Domain.Service.Events
{
    public class Workbench : IDisposable, ISerializable<WorkbenchMemento>, IPlayerEventEntity, IIconEntity
    {
        private ReactiveProperty<int> _remainingUsages;
        public ReadOnlyReactiveProperty<bool> CanUse => _remainingUsages.Select(remainingUsages => remainingUsages > 0).ToReadOnlyReactiveProperty();
        public EntityBase Entity { get; init; }
        public bool IsGrounded => true;

        public Workbench(WorkbenchMemento data)
        {
            Entity = new EntityBase(data.Entity);
            _remainingUsages = new ReactiveProperty<int>(data.RemainingUsages);
            Events = new List<IPlayerEvent>
            {
                new PlayerEvent(
                    "工作台を見つけた",
                    new List<PlayerChoiceEvent>
                    {
                        new(
                            "アイテムを修理する",
                            (player, map) => CanUse.CurrentValue,
                            async (gameManager, map) => await DoRepairEvent(gameManager, map)
                        ),
                        new(
                            "アイテムを強化する",
                            (player, map) => CanUse.CurrentValue,
                            async (gameManager, map) => await DoUpgradeEvent(gameManager, map)
                        )
                    }
                )
            };
        }

        public void Dispose()
        {
            Entity.Dispose();
        }

        public Sprite Icon => ObjectLoader.LoadMapChip("(Base)BaseChip_pipo_683");

        public IReadOnlyList<IPlayerEvent> Events { get; init; }

        private bool CanRepair(IItem item)
        {
            return item.RemainingUses.CurrentValue < item.MaxUsages;
        }

        private async UniTask DoRepairEvent(IGameManager gameManager, IMap map)
        {
            var player = map.Player;
            var itemIndex = await player.Character.SelectItemWithCanSelect(
                "修理するアイテムを選択してください",
                CanRepair);
            if (itemIndex == null)
                return;
            var item = player.Character.Inventory.GetItem(itemIndex.Value);
            if (item.ShouldRevealMimic(player.Character, player.Character.Entity.CurrentPosition, map))
            {
                return;
            }
            if (!player.Character.Inventory.CanRemove(item))
            {
                GameLog.AddIgnoreVisibility($"{item.GetName(player, map.ItemPlaceholders)}は取り出せなかった");
                return;
            }
            item.Repair(player, player.Character, map.ItemPlaceholders);
            gameManager.PlaySE(SE.WorkbenchCraft);
            _remainingUsages.Value -= 1;
        }

        private bool CanUpgrade(IItem item)
        {
            return item.CanUpgrade();
        }

        private async UniTask DoUpgradeEvent(IGameManager gameManager, IMap map)
        {
            var player = map.Player;
            var itemIndex = await player.Character.SelectItemWithCanSelectPreview(
                "強化するアイテムを選択してください",
                CanUpgrade,
                item =>
                {
                    if (!item.CanUpgrade())
                    {
                        return null;
                    }

                    var previewItem = item.Clone();
                    previewItem.Upgrade(player, player.Character, map.ItemPlaceholders, log: false);
                    return new ItemSelectPreview(new ItemFocus(0), previewItem, null);
                },
                defaultPreview: null,
                "<b>強化結果...</b>");
            if (itemIndex == null)
                return;
            var item = player.Character.Inventory.GetItem(itemIndex.Value);
            if (item.ShouldRevealMimic(player.Character, player.Character.Entity.CurrentPosition, map))
            {
                return;
            }
            if (!player.Character.Inventory.CanRemove(item))
            {
                GameLog.AddIgnoreVisibility($"{item.GetName(player, map.ItemPlaceholders)}は取り出せなかった");
                return;
            }
            item.Upgrade(player, player.Character, map.ItemPlaceholders);
            gameManager.PlaySE(SE.WorkbenchCraft);
            _remainingUsages.Value -= 1;
        }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            return UniTask.CompletedTask;
        }

        public WorkbenchMemento Serialize()
        {
            return new WorkbenchMemento(_remainingUsages.CurrentValue, Entity.Serialize());
        }

        public static WorkbenchMemento Build(Vector2Int position)
        {
            return new WorkbenchMemento(3, EntityBase.Build(position, EntityLayer.Middle));
        }
    }
}