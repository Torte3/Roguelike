using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Model.Memento;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Events
{
    public class Bonfire : ISerializable<BonfireMemento>, IPlayerEventEntity
    {
        public EntityBase Entity { get; init; }
        private readonly ReactiveProperty<bool> _isFire;
        public ReadOnlyReactiveProperty<bool> IsFire => _isFire;

        public Bonfire(BonfireMemento memento)
        {
            Entity = new EntityBase(memento.Entity);
            _isFire = new(memento.IsFire);
            Events = new List<IPlayerEvent>
            {
                new PlayerEvent(
                    "焚き火を見つけた",
                    new List<PlayerChoiceEvent>
                    {
                        new(
                            "休憩する",
                            (player, map) => _isFire.CurrentValue,
                            (gameManager, map) =>
                            {
                                map.Player.Character.RestoreToFullHealth();
                                gameManager.PlaySE(SE.BonfireRest);
                                _isFire.Value = false;
                                return UniTask.CompletedTask;
                            }
                        )
                    }
                )
            };
        }

        public IReadOnlyList<IPlayerEvent> Events { get; init; }

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
            return new BonfireMemento(IsFire.CurrentValue, Entity.Serialize());
        }

        public static BonfireMemento Build(Vector2Int position)
        {
            return new BonfireMemento(true, EntityBase.Build(position, EntityLayer.Middle));
        }
    }
}