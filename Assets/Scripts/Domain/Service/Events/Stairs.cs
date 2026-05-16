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
using ObservableCollections;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Events
{
    public class Stairs : IDisposable, ISerializable<StairsMemento>, IPlayerEventEntity, IMovementEntity, ILockedEntity
    {
        public MovementEntityType Type { get; init; }
        public Id<IMap> Destination { get; init; }
        public EntityBase Entity { get; init; }
        public bool IsGrounded => true;
        public Id<IEntity> DestinationId { get; init; }
        public List<Id<IEntity>> KeyCharacters { get; init; }
        private const string _keyBaseName = "黄金の鍵";

        public Stairs(StairsMemento data)
        {
            Type = data.Type;
            Entity = new EntityBase(data.Entity);
            Destination = data.Destination;
            DestinationId = data.DestinationId;
            KeyCharacters = data.KeyCharacters;
            
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
                            (player, map) => CanExecuteEvent(map),
                            (gameManager, map) => DoEvent(gameManager)
                        ),
                        new(
                            "鍵を使って進む",
                            (player, map) => !CanExecuteEvent(map) && CanUseKey(player),
                            (gameManager, map) => DoUseKeyEvent(gameManager, map)
                        )
                    }
                )
            };
        }

        public void Dispose()
        {
            Entity.Dispose();
        }

        public IReadOnlyList<IPlayerEvent> Events { get; init; }

        private bool CanExecuteEvent(IMap map)
        {
            return KeyCharacters.All(keyCharacterId => map.Characters.ById(keyCharacterId) == null);
        }

        private bool CanUseKey(IPlayer player)
        {
            return player.Character.Inventory.Contains(_keyBaseName);
        }

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
            gameManager.MoveMap(Destination, DestinationId);
            return UniTask.CompletedTask;
        }

        private UniTask DoUseKeyEvent(IGameManager gameManager, IMap map)
        {
            var player = map.Player;
            player.Character.Inventory.Remove(_keyBaseName);
            return DoEvent(gameManager);
        }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            return UniTask.CompletedTask;
        }

        public StairsMemento Serialize()
        {
            return new StairsMemento
            (
                Type,
                Destination,
                entity: Entity.Serialize(),
                destinationId: DestinationId,
                keyCharacters: KeyCharacters.ToList()
            );
        }

        public static StairsMemento Build(MovementEntityType type, Vector2Int position, Id<IEntity> id,
            Id<IMap> destination, Id<IEntity> destinationId, List<Id<IEntity>> keyCharacters)
        {
            return new StairsMemento
            (
                type,
                destination,
                entity: EntityBase.Build(id, position, EntityLayer.Floor),
                destinationId: destinationId,
                keyCharacters: keyCharacters
            );
        }

        public static StairsMemento Build(MovementEntityType type, Vector2Int position, Id<IMap> destination, List<Id<IEntity>> keyCharacters)
        {
            return Build(type, position, Id<IEntity>.Generate(), destination, Id<IEntity>.Generate(), keyCharacters);
        }
    }
}