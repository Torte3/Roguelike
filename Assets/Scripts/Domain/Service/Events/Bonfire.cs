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
using UnityEngine;
using Utilities;

namespace Domain.Service.Events
{
    public class Bonfire : ISerializable<BonfireMemento>, IPlayerEventEntity
    {
        public EntityBase Entity { get; init; }
        public bool IsGrounded => true;
        private int _remainingUsages;
        private bool CanUse => _remainingUsages > 0;

        public Bonfire(BonfireMemento memento)
        {
            Entity = new EntityBase(memento.Entity);
            _remainingUsages = memento.RemainingUsages;
            Events = new List<IPlayerEvent>
            {
                new PlayerEvent(
                    FixtureKind.Bonfire,
                    new List<PlayerChoiceEvent>
                    {
                        new(
                            "休憩する",
                            (player, map) => CanUse,
                            (gameManager, map) =>
                            {
                                map.Events.Record(new FacilityUsed(FixtureKind.Bonfire));
                                map.Player.Character.RestoreToFullHealth();
                                ConsumeUse();
                                return UniTask.CompletedTask;
                            }
                        ),
                        new(
                            "呪いを解く",
                            (player, map) => CanUse,
                            async (_, map) => await DoUncurseEvent(map)
                        )
                    }
                )
            };
        }

        public IReadOnlyList<IPlayerEvent> Events { get; init; }

        private void ConsumeUse()
        {
            _remainingUsages -= 1;
            if (!CanUse)
                Entity.Record(new FacilityExhausted(Entity.Ref, null));
        }

        private async UniTask DoUncurseEvent(IMap map)
        {
            var player = map.Player;
            var itemIndex = await player.Character.SelectItemWithCanSelect(
                "呪いを解くアイテムを選択してください",
                item => item.IsCursed || (!player.Character.IsKnownItem(item) && !item.IsCurseIdentified));
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
            map.Events.Record(new FacilityUsed(FixtureKind.Bonfire));
            item.SetCursed(player.Character, map, false);
            ConsumeUse();
        }

        public EntityLabel LabelIn(IMap map)
        {
            return new KindEntityLabel(FixtureKind.Bonfire);
        }

        public WorldEvent Appeared(IMap map)
        {
            return new FacilityAppeared(Entity.Ref, Entity.AppearanceOf(FixtureKind.Bonfire.ToEntityKind(), null), CanUse);
        }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            return UniTask.CompletedTask;
        }

        public void Dispose()
        {
            Entity.Dispose();
        }

        public BonfireMemento Serialize()
        {
            return new BonfireMemento(_remainingUsages, Entity.Serialize());
        }

        public static BonfireMemento Build(Vector2Int position)
        {
            return new BonfireMemento(3, EntityBase.Build(position, EntityLayer.Middle));
        }
    }
}