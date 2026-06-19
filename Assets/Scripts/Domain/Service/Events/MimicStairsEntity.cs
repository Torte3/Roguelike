using System;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Logs;
using UnityEngine;
using Utilities;

namespace Domain.Service.Events
{
    public class MimicStairs : IDisposable, ISerializable<MimicStairsMemento>, ICharacterEventEntity
    {
        public MovementEntityType Type { get; init; }
        public EntityBase Entity { get; init; }
        public EnemyData Mimic { get; init; }
        public bool IsGrounded => true;

        public MimicStairs(MimicStairsMemento data)
        {
            Type = data.Type;
            Entity = new EntityBase(data.Entity);
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
            var entityName = Type switch
            {
                MovementEntityType.UpStairs => "階段",
                MovementEntityType.DownStairs => "階段",
                MovementEntityType.MagicCircle => "魔法陣",
                _ => throw new NotImplementedException(),
            };
            GameLog.Add(map.Player.Character.IsVisible(Entity.CurrentPosition), $"{entityName}は{Mimic.Name}の擬態だった！");
            Entity.Destroy("モンスターが正体を表した");
            return map.SpawnEnemyIgnoreMimic(
                Mimic,
                Entity.CurrentPosition,
                doActImmediately: true,
                isSlept: false,
                isShiny: false
            );
        }

        public void Dispose()
        {
            Entity.Dispose();
        }

        public ICharacterEvent Event { get; init; }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            return UniTask.CompletedTask;
        }

        public MimicStairsMemento Serialize()
        {
            return new MimicStairsMemento(Type, Entity.Serialize(), Mimic);
        }

        public static MimicStairsMemento Build(MovementEntityType type, Vector2Int position, EnemyData mimic)
        {
            return new MimicStairsMemento(type, EntityBase.Build(position, EntityLayer.Floor, ignoreGrass: true), mimic);
        }
    }
}