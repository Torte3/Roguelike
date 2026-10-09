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
using Utilities;

namespace Domain.Service.Events
{
    public class Workbench : IDisposable, ISerializable<WorkbenchMemento>, IPlayerEventEntity
    {
        private int _remainingUsages;
        private bool CanUse => _remainingUsages > 0;
        public EntityBase Entity { get; init; }
        public bool IsGrounded => true;

        public Workbench(WorkbenchMemento data)
        {
            Entity = new EntityBase(data.Entity);
            _remainingUsages = data.RemainingUsages;
            Events = new List<IPlayerEvent>
            {
                new PlayerEvent(
                    FixtureKind.Workbench,
                    new List<PlayerChoiceEvent>
                    {
                        new(
                            "アイテムを修理する",
                            (player, map) => CanUse,
                            async (_, map) => await DoRepairEvent(map)
                        ),
                        new(
                            "アイテムを強化する",
                            (player, map) => CanUse,
                            async (_, map) => await DoUpgradeEvent(map)
                        )
                    }
                )
            };
        }

        public void Dispose()
        {
            Entity.Dispose();
        }

        private Sprite Icon => ObjectLoader.LoadMapChip("(Base)BaseChip_pipo_683");

        public IReadOnlyList<IPlayerEvent> Events { get; init; }

        private bool CanRepair(IItem item)
        {
            return item.RemainingUses.CurrentValue < item.MaxUsages;
        }

        private async UniTask DoRepairEvent(IMap map)
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
            if (player.Character.Inventory.TakeOutCheck(item).IsFailed(out var failure))
            {
                map.Events.Record(new FacilityItemFailed(item.NameIn(map), failure));
                return;
            }
            map.Events.Record(new FacilityUsed(FixtureKind.Workbench));
            item.Repair(player.Character, map);
            ConsumeUse();
        }

        private void ConsumeUse()
        {
            _remainingUsages -= 1;
            if (!CanUse)
                Entity.Record(new FacilityExhausted(Entity.Ref, null));
        }

        private bool CanUpgrade(IItem item)
        {
            return item.CanUpgrade();
        }

        private async UniTask DoUpgradeEvent(IMap map)
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
                    previewItem.Upgrade(player.Character, map, log: false);
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
            if (player.Character.Inventory.TakeOutCheck(item).IsFailed(out var failure))
            {
                map.Events.Record(new FacilityItemFailed(item.NameIn(map), failure));
                return;
            }
            map.Events.Record(new FacilityUsed(FixtureKind.Workbench));
            item.Upgrade(player.Character, map);
            ConsumeUse();
        }

        public EntityLabel LabelIn(IMap map)
        {
            return new KindEntityLabel(FixtureKind.Workbench);
        }

        public WorldEvent Appeared(IMap map)
        {
            return new FacilityAppeared(Entity.Ref, Entity.AppearanceOf(FixtureKind.Workbench.ToEntityKind(), Icon), CanUse);
        }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            return UniTask.CompletedTask;
        }

        public WorkbenchMemento Serialize()
        {
            return new WorkbenchMemento(_remainingUsages, Entity.Serialize());
        }

        public static WorkbenchMemento Build(Vector2Int position)
        {
            return new WorkbenchMemento(3, EntityBase.Build(position, EntityLayer.Middle));
        }
    }
}