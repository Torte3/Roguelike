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
    public class MimicMoney : IDisposable, ICharacterEventEntity, IIconEntity
    {
        private readonly Money _money;
        public EntityBase Entity => _money.Entity;
        public EnemyData Mimic { get; init; }
        public bool IsGrounded => true;

        public MimicMoney(MimicMoneyMemento data)
        {
            _money = new Money(data.Money);
            Mimic = data.Mimic.Value;
            Event = new CharacterEvent(
                character => character.IsPlayer,
                (character, gameManager, map) =>
                {
                    Reveal(map);
                    return UniTask.CompletedTask;
                }
            );
        }

        public ICharacter Reveal(IMap map)
        {
            GameLog.Add(map.Player.Character.IsVisible(Entity.CurrentPosition), $"{_money.Amount}Gはモンスターだった");
            Entity.Destroy("モンスターが正体を表した");
            return map.SpawnEnemyIgnoreMimic(
                Mimic,
                _money.Entity.CurrentPosition,
                doActImmediately: true,
                isSlept: false,
                isShiny: false
            );
        }

        public void Dispose()
        {
            _money.Dispose();
        }

        public Sprite Icon => _money.Icon;

        public ICharacterEvent Event { get; init; }

        public void SetVisibility(bool visibility)
        {
            _money.SetVisibility(visibility);
        }

        public async UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            var destination = ItemEntity.GetThrowDestination(_money.Entity.CurrentPosition, direction, distance, map);
            if (Entity.Visibility.CurrentValue && destination != Entity.CurrentPosition)
            {
                Entity.SetVisibility(false);
                await map.ShowThrowAnimation(Icon, Entity.CurrentPosition, direction, distance, false, EntityLayer.Middle);
                Entity.Teleport(map.FindBlankPositionFrom(destination,
                    position => map.At(position).IsBlankAndStandable(EntityLayer.Bottom)));
            }
        }

        public MimicMoneyMemento Serialize()
        {
            return new MimicMoneyMemento(_money.Serialize(), Mimic);
        }

        public static MimicMoneyMemento Build(Vector2Int position, int amount, EnemyData mimic)
        {
            return new MimicMoneyMemento(Money.Build(position, amount), mimic);
        }
    }
}