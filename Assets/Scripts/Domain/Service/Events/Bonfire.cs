using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Logs;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Events
{
    public class Bonfire : ISerializable<BonfireMemento>, IPlayerEventEntity
    {
        public EntityBase Entity { get; init; }
        public bool IsGrounded => true;
        private readonly ReactiveProperty<int> _remainingUsages;
        public ReadOnlyReactiveProperty<bool> CanUse => _remainingUsages.Select(remainingUsages => remainingUsages > 0).ToReadOnlyReactiveProperty();
        public ReadOnlyReactiveProperty<bool> IsFire => CanUse;

        public Bonfire(BonfireMemento memento)
        {
            Entity = new EntityBase(memento.Entity);
            _remainingUsages = new(memento.RemainingUsages);
            Events = new List<IPlayerEvent>
            {
                new PlayerEvent(
                    "焚き火を見つけた",
                    new List<PlayerChoiceEvent>
                    {
                        new(
                            "休憩する",
                            (player, map) => CanUse.CurrentValue,
                            (gameManager, map) =>
                            {
                                map.Player.Character.RestoreToFullHealth();
                                gameManager.PlaySE(SE.BonfireRest);
                                ConsumeUse();
                                return UniTask.CompletedTask;
                            }
                        ),
                        new(
                            "呪いを解く",
                            (player, map) => CanUse.CurrentValue,
                            async (gameManager, map) => await DoUncurseEvent(gameManager, map)
                        )
                    }
                )
            };
        }

        public IReadOnlyList<IPlayerEvent> Events { get; init; }

        private void ConsumeUse()
        {
            _remainingUsages.Value -= 1;
        }

        private async UniTask DoUncurseEvent(IGameManager gameManager, IMap map)
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
            if (!player.Character.Inventory.CanRemove(item))
            {
                GameLog.AddIgnoreVisibility($"{item.GetName(player, map.ItemPlaceholders)}は取り出せなかった");
                return;
            }
            item.SetCursed(player, player.Character, map.ItemPlaceholders, false);
            gameManager.PlaySE(SE.BonfireRest);
            ConsumeUse();
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
            return new BonfireMemento(_remainingUsages.CurrentValue, Entity.Serialize());
        }

        public static BonfireMemento Build(Vector2Int position)
        {
            return new BonfireMemento(3, EntityBase.Build(position, EntityLayer.Middle));
        }
    }
}