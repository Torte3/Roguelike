#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Characters.Behavior;
using R3;
using Unity.Logging;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using VContainer;

namespace Game
{
    public class World : ISerializable<WorldMemento>
    {
        private ReactiveProperty<MapManager> _activeMap = new();
        private Id<IMap> _activeMapId => _activeMap.CurrentValue.Id;
        private ReactiveProperty<bool> _isLoaded = new(false);
        private Subject<OnActiveMapChangedMessage> _onActiveMapChanged = new();
        private Dictionary<Id<IMap>, MapMemento> _maps = new();
        private HashSet<Id<IMap>> _updatedMapIds = new();
        private Dungeon _dungeon;
        private ItemPlaceholders _itemPlaceholders;
        private Placeholders _placeholders;
        private ItemMarketPriceTable _marketPriceTable;
        private CharacterControlInputReceiver _receiver;

        [Inject]
        public World(CharacterControlInputReceiver receiver)
        {
            _receiver = receiver;
            _placeholders = Addressables.LoadAssetAsync<Placeholders>("Assets/Database/ItemData/Placeholders.asset")
                .WaitForCompletion();
            _marketPriceTable = Addressables.LoadAssetAsync<ItemMarketPriceTable>("Assets/Database/ItemData/ItemMarketPriceTable.asset")
                .WaitForCompletion();
            _activeMap.SkipLatestValueOnSubscribe().Pairwise().Subscribe(map =>
            {
                map.Previous.Dispose();
                _updatedMapIds.Add(map.Current.Id);
            });
        }

        public void CreateNew()
        {
            var blueprint = ObjectLoader.Load<DungeonBluePrintData>("Dungeon");
            _dungeon = new Dungeon(blueprint);
            _dungeon.InitializeNewGame();
            _itemPlaceholders = new ItemPlaceholders(ItemPlaceholders.Build(), _placeholders);
            _maps = new Dictionary<Id<IMap>, MapMemento>();
            _updatedMapIds = new HashSet<Id<IMap>>();
            _isLoaded.Value = false;
        }

        public WorldMemento Serialize()
        {
            var playerData = _activeMap.CurrentValue.Player.Serialize();
            var partyMembers = _activeMap.CurrentValue.GetFollowingCharacters().Select(character => character.Serialize()).ToList();
            return new WorldMemento
            (
                _dungeon.Serialize(),
                playerData,
                partyMembers,
                _activeMap.CurrentValue.Player.Character.IsDead,
                _maps.Keys.ToList(),
                _activeMapId,
                _itemPlaceholders.Serialize()
            );
        }

        public List<MapMemento> SerializeUpdatedMaps()
        {
            var activeMapMemento = _activeMap.CurrentValue.SerializeWithoutPartyMembers();
            _maps[_activeMapId] = activeMapMemento;
            _updatedMapIds.Add(_activeMapId);
            var updatedMaps = _updatedMapIds.Select(mapId => _maps[mapId]).ToList();
            _updatedMapIds.Clear();
            _updatedMapIds.Add(_activeMapId);
            return updatedMaps;
        }

        public MapManager? CurrentMap => _isLoaded.CurrentValue ? _activeMap.CurrentValue : null;
        public Observable<OnActiveMapChangedMessage> OnActiveMapChanged => _onActiveMapChanged;

        private MapMemento GetMapMemento(Id<IMap> mapId)
        {
            if (_dungeon.ShouldBatchCreateSection(mapId))
            {
                var sectionMapIds = _dungeon.GetSectionMapIds(mapId);
                foreach (var sectionMapId in sectionMapIds)
                {
                    if (!_maps.ContainsKey(sectionMapId))
                    {
                        _maps[sectionMapId] = CreateMap(sectionMapId);
                        _updatedMapIds.Add(sectionMapId);
                    }
                }
            }
            else if (!_maps.ContainsKey(mapId))
            {
                _maps[mapId] = CreateMap(mapId);
            }
            return _maps[mapId];
        }

        private MapMemento CreateMap(Id<IMap> id)
        {
            List<MovementData> movementData = new();
            var destinations = _dungeon.GetDestinations(id);
            foreach (var destination in destinations)
            {
                movementData.Add(ResolveMovementData(destination.Type, id, destination.Destination));
            }

            return _dungeon.CreateMapManager(id, movementData);
        }

        private MovementData ResolveMovementData(MovementEntityType type, Id<IMap> current, Id<IMap> destination)
        {
            if (_maps.TryGetValue(destination, out var destinationMap))
            {
                var peerStairs = destinationMap.Entities.EventEntities.Stairs
                    .FirstOrDefault(s => s.Destination == current && s.Type == type.Reverse());
                if (peerStairs != null)
                {
                    return new MovementData(
                        type,
                        destination,
                        peerStairs.DestinationId,
                        new Id<IEntity>(peerStairs.Entity.Id));
                }
            }

            var idOnCurrent = Id<IEntity>.Generate();
            var idOnDestination = Id<IEntity>.Generate();
            return new MovementData(type, destination, idOnCurrent, idOnDestination);
        }

        public void SetActiveMap(MapManager map, bool isNewWorld)
        {
            _isLoaded.Value = true;
            var previousMap = _activeMap.CurrentValue;
            _activeMap.Value = map;
            _onActiveMapChanged.OnNext(new OnActiveMapChangedMessage(map, previousMap, isNewWorld));
        }

        public MapManager LoadWorld(WorldMemento memento, Dictionary<Id<IMap>, MapMemento> maps, IGameManager gameManager, bool isNewWorld)
        {
            _dungeon = new Dungeon(memento.Dungeon);
            _itemPlaceholders = new ItemPlaceholders(memento.ItemPlaceholders, _placeholders);
            _maps = memento.MapIds.ToDictionary(
                mapId => mapId,
                mapId => maps[mapId]
            );

            _updatedMapIds = new HashSet<Id<IMap>> { memento.CurrentMapId };

            Log.Debug($"LoadMap mapId:{memento.CurrentMapId}");
            var mapMemento = GetMapMemento(memento.CurrentMapId);

            MapManager map = CreateMapManagerFromSave(mapMemento, memento.CurrentMapId, memento.Player, memento.PartyMembers,
                memento.Player.Character.Entity.Position, false, gameManager);

            SetActiveMap(map, isNewWorld);

            return map;
        }

        public MapManager LoadStartMap(PlayerData playerData, IGameManager gameManager)
        {
            return LoadMap(_dungeon.StartMapId, playerData, gameManager);
        }

        public MapManager LoadMap(Id<IMap> mapId, PlayerData playerData, IGameManager gameManager)
        {
            Log.Debug($"LoadMap mapId:{mapId}");
            var mapMemento = GetMapMemento(mapId);

            var map = CreateMapManagerForNewGame(mapMemento, mapId, playerData, gameManager);

            SetActiveMap(map, true);

            return map;
        }

        public MapManager LoadMap(Id<IMap> mapId, Id<IEntity>? destination, IGameManager gameManager)
        {
            Log.Debug($"LoadMap mapId:{mapId}");

            if (CurrentMap == null)
                throw new Exception("CurrentMap is null");

            var fromMapId = _activeMapId;
            _maps[fromMapId] = CurrentMap.SerializeWithoutPartyMembers();

            var mapMemento = GetMapMemento(mapId);

            var playerMemento = CurrentMap.Player.Serialize();
            var partyMembers = CurrentMap.GetFollowingCharacters()
                .Select(character => character.Serialize()).ToList();
            Vector2Int? initialPosition = destination != null
                ? mapMemento.Entities.EventEntities.Stairs.First(s => s.Entity.Id == destination.ToString()).Entity.Position
                : null;

            var map = CreateMapManagerFromSave(mapMemento, mapId, playerMemento, partyMembers, initialPosition, true, gameManager);

            SetActiveMap(map, false);

            return map;
        }

        private MapManager CreateMapManagerForNewGame(
            MapMemento mapMemento,
            Id<IMap> mapId,
            PlayerData playerData,
            IGameManager gameManager)
        {
            var spec = _dungeon.GetFloorSpec(mapId);
            var depth = _dungeon.GetDepth(mapId);
            var progress = _dungeon.GetProgress(mapId);
            return new MapManager(mapMemento, spec, depth, progress, playerData, gameManager, _receiver, _itemPlaceholders, _marketPriceTable);
        }

        private MapManager CreateMapManagerFromSave(
            MapMemento mapMemento,
            Id<IMap> mapId,
            PlayerMemento playerMemento,
            List<CharacterMemento> partyMembers,
            Vector2Int? initialPosition,
            bool resetPartyPositions,
            IGameManager gameManager)
        {
            var spec = _dungeon.GetFloorSpec(mapId);
            var depth = _dungeon.GetDepth(mapId);
            var progress = _dungeon.GetProgress(mapId);
            return new MapManager(mapMemento, spec, depth, progress, playerMemento,
                partyMembers, initialPosition, resetPartyPositions, gameManager, _receiver, _itemPlaceholders, _marketPriceTable);
        }
    }
}