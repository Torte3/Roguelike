#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.WorldEvents;
using UnityEngine;
using Utilities;

namespace Domain.Service.Events
{
    public class Stairs : IDisposable, ISerializable<StairsMemento>, IPlayerEventEntity, IMovementEntity
    {
        private const string ActiveMagicCircleMapChipName = "(Base)BaseChip_pipo_71";
        private const string UsedMagicCircleMapChipName = "(Base)BaseChip_pipo_70";

        public MovementEntityType Type { get; init; }
        private FixtureKind FixtureKind => Type.ToFixtureKind();
        public Id<IMap> Destination { get; init; }
        public EntityBase Entity { get; init; }
        public bool IsGrounded => true;
        public Id<IEntity> DestinationId { get; init; }

        private bool _isUsed;
        private bool CanUse => Type != MovementEntityType.MagicCircle || !_isUsed;

        public Stairs(StairsMemento data)
        {
            Type = data.Type;
            Entity = new EntityBase(data.Entity);
            Destination = data.Destination;
            DestinationId = data.DestinationId;
            _isUsed = Type == MovementEntityType.MagicCircle && data.IsUsed;

            Events = new List<IPlayerEvent>
            {
                new PlayerEvent(
                    map => HasUnopenedUnlockedChest(map)
                        ? new ChoiceMessage("まだ鍵の開いた宝箱を開けていない！", TextTone.Caution)
                        : PlayerEvent.FoundMessage(FixtureKind),
                    new List<PlayerChoiceEvent>
                    {
                        new(
                            "進む",
                            (player, map) => CanUse,
                            (gameManager, map) => DoEvent(gameManager, map)),
                    }),
            };
        }

        public void Dispose()
        {
            Entity.Dispose();
        }

        public IReadOnlyList<IPlayerEvent> Events { get; init; }

        private Sprite? Icon => IconOf(Type, CanUse);

        internal static Sprite? IconOf(MovementEntityType type, bool canUse)
        {
            if (type != MovementEntityType.MagicCircle)
                return null;
            return ObjectLoader.LoadMapChip(canUse ? ActiveMagicCircleMapChipName : UsedMagicCircleMapChipName);
        }

        public EntityLabel LabelIn(IMap map)
        {
            return new KindEntityLabel(FixtureKind);
        }

        public WorldEvent Appeared(IMap map)
        {
            return new FacilityAppeared(Entity.Ref, Entity.AppearanceOf(FixtureKind.ToEntityKind(), Icon), CanUse);
        }

        private static bool HasUnopenedUnlockedChest(IMap map)
        {
            return map.LockedEntities.Any(locked => locked.IsLockReleased);
        }

        private UniTask DoEvent(IGameManager gameManager, IMap map)
        {
            map.Events.Record(new FacilityUsed(FixtureKind));
            if (Type == MovementEntityType.MagicCircle)
                _isUsed = true;
            gameManager.MoveMap(Destination, DestinationId);
            return UniTask.CompletedTask;
        }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map) =>
            UniTask.CompletedTask;

        public StairsMemento Serialize() =>
            new(
                Type,
                Destination,
                DestinationId,
                Entity.Serialize(),
                Type == MovementEntityType.MagicCircle && _isUsed);

        public static StairsMemento Build(
            MovementEntityType type,
            Vector2Int position,
            Id<IEntity> id,
            Id<IMap> destination,
            Id<IEntity> destinationId) =>
            new(
                type,
                destination,
                destinationId,
                EntityBase.Build(id, position, EntityLayer.Floor, ignoreGrass: true),
                false);

        public static StairsMemento Build(
            MovementEntityType type,
            Vector2Int position,
            Id<IMap> destination) =>
            Build(type, position, Id<IEntity>.Generate(), destination, Id<IEntity>.Generate());
    }
}
