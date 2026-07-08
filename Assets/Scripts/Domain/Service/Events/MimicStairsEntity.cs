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
    public class MimicStairs : IDisposable, ISerializable<MimicStairsMemento>, ICharacterEventEntity
    {
        public MovementEntityType Type { get; init; }
        private FixtureKind FixtureKind => Type.ToFixtureKind();
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
            map.Events.Record(new MimicRevealed(Entity.IsVisible, LabelIn(map), Mimic.Name, null));
            Entity.Destroy();
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

        public EntityLabel LabelIn(IMap map)
        {
            return new KindEntityLabel(FixtureKind);
        }

        public WorldEvent Appeared(IMap map)
        {
            return new FacilityAppeared(Entity.Ref, Entity.AppearanceOf(FixtureKind.ToEntityKind(), Stairs.IconOf(Type, true)), true);
        }

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