#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Model.Memento;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Events
{
    public class Stairs : IDisposable, ISerializable<StairsMemento>, IPlayerEventEntity, IMovementEntity
    {
        public const string ActiveMagicCircleMapChipName = "(Base)BaseChip_pipo_71";
        public const string UsedMagicCircleMapChipName = "(Base)BaseChip_pipo_70";

        public MovementEntityType Type { get; init; }
        public Id<IMap> Destination { get; init; }
        public EntityBase Entity { get; init; }
        public bool IsGrounded => true;
        public Id<IEntity> DestinationId { get; init; }

        private readonly ReactiveProperty<bool> _isUsed;
        public ReadOnlyReactiveProperty<bool> CanUse { get; }

        public Stairs(StairsMemento data)
        {
            Type = data.Type;
            Entity = new EntityBase(data.Entity);
            Destination = data.Destination;
            DestinationId = data.DestinationId;
            _isUsed = new ReactiveProperty<bool>(
                Type == MovementEntityType.MagicCircle && data.IsUsed);
            CanUse = _isUsed
                .Select(used => Type != MovementEntityType.MagicCircle || !used)
                .ToReadOnlyReactiveProperty();

            var entityName = Type switch
            {
                MovementEntityType.UpStairs => "階段",
                MovementEntityType.DownStairs => "階段",
                MovementEntityType.MagicCircle => "魔法陣",
                _ => throw new NotImplementedException(),
            };
            Events = new List<IPlayerEvent>
            {
                new PlayerEvent(
                    $"{entityName}を見つけた",
                    new List<PlayerChoiceEvent>
                    {
                        new(
                            "進む",
                            (player, map) => CanUse.CurrentValue,
                            (gameManager, map) => DoEvent(gameManager)),
                    }),
            };
        }

        public void Dispose()
        {
            Entity.Dispose();
        }

        public IReadOnlyList<IPlayerEvent> Events { get; init; }

        private UniTask DoEvent(IGameManager gameManager)
        {
            var se = Type switch
            {
                MovementEntityType.UpStairs => SE.Stairs,
                MovementEntityType.DownStairs => SE.Stairs,
                MovementEntityType.MagicCircle => SE.Teleport,
                _ => SE.Stairs,
            };
            gameManager.PlaySE(se);
            if (Type == MovementEntityType.MagicCircle)
                _isUsed.Value = true;
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
                Type == MovementEntityType.MagicCircle && _isUsed.CurrentValue);

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
