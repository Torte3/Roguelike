using System;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.WorldEvents;
using Domain.Service.Items;
using UnityEngine;
using Utilities;

namespace Domain.Service.Events
{
    public class Money : IDisposable, ISerializable<MoneyMemento>, ICharacterEventEntity
    {
        public EntityBase Entity { get; init; }
        public readonly int Amount;
        public bool IsGrounded => true;

        public Money(MoneyMemento data)
        {
            Entity = new EntityBase(data.Entity);
            Amount = data.Amount;
            Event = new CharacterEvent(
                character => character.IsPlayer,
                (character, gameManager, map) =>
                {
                    map.Player.AddMoney(Amount);
                    map.Events.Record(new MoneyPickedUp(map.Player.Character.Label, Amount, map.Player.Money,
                        new ObtainedItem(Icon, Entity.CurrentPosition)));
                    Entity.Destroy();
                    return UniTask.CompletedTask;
                }
            );
        }

        public void Dispose()
        {
            Entity.Dispose();
        }

        private Sprite Icon => Amount switch
        {
            <= 100 => ObjectLoader.LoadIcon("icons_full_16_362"),
            <= 300 => ObjectLoader.LoadIcon("icons_full_16_363"),
            <= 1000 => ObjectLoader.LoadIcon("icons_full_16_360"),
            <= 3000 => ObjectLoader.LoadIcon("icons_full_16_361"),
            <= 10000 => ObjectLoader.LoadIcon("icons_full_16_365"),
            <= 30000 => ObjectLoader.LoadIcon("icons_full_16_366"),
            _ => ObjectLoader.LoadIcon("icons_full_16_358")
        };

        public ICharacterEvent Event { get; init; }

        public void SetVisibility(bool visibility)
        {
            Entity.SetVisibility(visibility);
        }

        public bool CanBeBrokenBy(BreakTargets targets) => targets.HasFlag(BreakTargets.Money);

        public EntityLabel LabelIn(IMap map)
        {
            return new MoneyEntityLabel(Amount);
        }

        public WorldEvent Appeared(IMap map)
        {
            return Entity.Appeared(EntityKind.Money, Icon);
        }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            var destination = ItemEntity.GetThrowDestination(Entity.CurrentPosition, direction, distance, map);
            if (ItemEntity.GetFloorLanding(Entity.CurrentPosition, destination, map) is { } landing)
                Entity.BlowTo(landing);

            return UniTask.CompletedTask;
        }

        public MoneyMemento Serialize()
        {
            return new MoneyMemento
            (
                Entity.Serialize(),
                Amount
            );
        }

        public static MoneyMemento Build(Vector2Int position, int amount)
        {
            return new MoneyMemento(EntityBase.Build(position, EntityLayer.Bottom), amount);
        }
    }
}