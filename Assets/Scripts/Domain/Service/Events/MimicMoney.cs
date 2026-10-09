using System;
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
    public class MimicMoney : IDisposable, ICharacterEventEntity
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
            map.Events.Record(new MimicRevealed(Entity.IsVisible, LabelIn(map), Mimic.Name, null));
            Entity.Destroy();
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

        public ICharacterEvent Event { get; init; }

        public void SetVisibility(bool visibility)
        {
            _money.SetVisibility(visibility);
        }

        public EntityLabel LabelIn(IMap map)
        {
            return _money.LabelIn(map);
        }

        public WorldEvent Appeared(IMap map)
        {
            return _money.Appeared(map);
        }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            return _money.BlowAway(actor, direction, distance, map);
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