using System;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Items;
using Domain.Service.Logs;
using UnityEngine;
using Utilities;

namespace Domain.Service.Events
{
    public class Money : IDisposable, ISerializable<MoneyMemento>, ICharacterEventEntity, IIconEntity
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
                    gameManager.PlaySE(SE.Pickup);
                    gameManager.RequestWorldIconPopup(Icon, Entity.CurrentPosition);
                    GameLog.AddIgnoreVisibility($"{map.Player.Character.GetName(map.Player)}は{Amount}Gを拾った");
                    Entity.Destroy($"は{map.Player.Character.GetName(map.Player)}に拾われた");
                    return UniTask.CompletedTask;
                }
            );
        }

        public void Dispose()
        {
            Entity.Dispose();
        }

        public Sprite Icon => Amount switch
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

        public async UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            var destination = ItemEntity.GetThrowDestination(Entity.CurrentPosition, direction, distance, map);
            if (Entity.Visibility.CurrentValue && destination != Entity.CurrentPosition)
            {
                Entity.SetVisibility(false);
                await map.ShowThrowAnimation(Icon, Entity.CurrentPosition, direction, distance, false, EntityLayer.Middle);
                Entity.Teleport(map.FindBlankPositionFrom(destination,
                    position => map.At(position).IsBlankAndStandable(EntityLayer.Bottom)));
            }
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