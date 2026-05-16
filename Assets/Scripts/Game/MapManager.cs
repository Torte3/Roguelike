#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Message;
using Domain.Model.Character.Status;
using Domain.Model.Dungeon;
using Domain.Model.Entity;
using Domain.Model.Evaluation;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.Setting;
using Domain.Service.Characters;
using Domain.Service.Characters.Behavior;
using Domain.Service.Events;
using Domain.Service.Items;
using Domain.Service.Logs;
using Domain.Service.Map;
using Domain.Service.Rooms;
using ObservableCollections;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;

namespace Game
{
    public class MapManager : IDisposable, ISerializable<MapMemento>, IMap
    {
        public Id<IMap> Id { get; init; }
        public string Name => "Dungeon";
        public int Depth => _dungeonData.Depth;
        public MapType Type => _dungeonData.Type;
        public ItemDatabase ItemDatabase => _dungeonData.ItemDatabase;
        public ItemPlaceholders ItemPlaceholders { get; init; }
        public ItemMarketPriceTable MarketPriceTable { get; init; }
        private readonly CompositeDisposable _disposables = new();
        private readonly ITilemap _tilemap;
        private DungeonMapData _dungeonData;
        private List<IEventArea> _rooms = new();
        private MonsterHouse? _monsterHouse;
        private Shop? _shop;
        public IShop? Shop => _shop;
        public ReadOnlyReactiveProperty<bool>? IsStolen => _shop?.IsStolen;
        public RectInt? ShopRect => _shop?.Rect;
        private readonly Subject<OnEffectSpawnedMessage> _onEffectSpawned = new();
        private readonly IGameManager _gameManager;
        public EntityManager EntityManager { get; init; }

        public MapManager(MapMemento map, DungeonMapData data, PlayerData playerData, IGameManager gameManager, CharacterControlInputReceiver receiver, ItemPlaceholders itemPlaceholders, ItemMarketPriceTable marketPriceTable)
        {
            Id = map.Id;
            ItemPlaceholders = itemPlaceholders;
            MarketPriceTable = marketPriceTable;
            _gameManager = gameManager;

            var playerPosition = map.RandomBlankPosition;

            _tilemap = new Tilemap(map.Tilemap);

            var playerMemento = CharacterFactory.BuildPlayer(playerData, playerPosition);

            EntityManager = new EntityManager(map.Entities, playerMemento, new(), playerPosition, false, receiver, gameManager, this);

            _dungeonData = data;

            if (map.MonsterHouse.HasValue)
            {
                _monsterHouse = new MonsterHouse(map.MonsterHouse.Value, EntityManager.Player.Character.Entity.CurrentPosition);
                _rooms.Add(_monsterHouse);
            }

            if (map.Shop.HasValue)
            {
                var clerk = EntityManager.Characters.FirstOrDefault(character =>
                    character.Entity.Id == map.Shop.Value.ClerkId);
                if (clerk == null && !map.Shop.Value.IsStolen)
                {
                    var clerkPosition = GetAllBlankAndStandablePositions()
                        .In(map.Shop.Value.Room.Room.RectRange())
                        .GetAtRandom();
                    clerk = EntityManager.SpawnCharacter(
                        CharacterFactory.BuildCharacter(
                            _dungeonData.Clerk,
                            clerkPosition.Position,
                            homeLocation: new Location(Id, clerkPosition.Position)),
                            gameManager,
                            this);
                }

                if (clerk != null)
                {
                    _shop = new Shop(map.Shop.Value, clerk, gameManager, this);
                    _rooms.Add(_shop);
                }
            }

            SetRules(gameManager);

            var visibleArea = EntityManager.Player.Character.VisionRange.VisibleArea;
            _tilemap.UpdateChunk(EntityManager.Player.Character.Entity.CurrentPosition);
            _tilemap.SetTilesKnown(visibleArea, true);

            UpdateVisibility(EntityManager.Entities);

            if (map.MonsterHouse.HasValue && !map.MonsterHouse.Value.HasEverEntered)
            {
                GameLog.AddIgnoreVisibility("<color=yellow>不穏な気配を感じる……</color>");
            }
        }

        public MapManager(MapMemento map, DungeonMapData data, PlayerMemento playerMemento,
            List<CharacterMemento> partyMembers,
            Vector2Int? playerPosition, bool resetPertyPositions, IGameManager gameManager, CharacterControlInputReceiver receiver, ItemPlaceholders itemPlaceholders, ItemMarketPriceTable marketPriceTable)
        {
            Id = map.Id;
            ItemPlaceholders = itemPlaceholders;
            MarketPriceTable = marketPriceTable;
            _gameManager = gameManager;

            if (playerPosition == null)
            {
                playerPosition = map.RandomBlankPosition;
            }

            _tilemap = new Tilemap(map.Tilemap);

            playerMemento = playerMemento.CopyWith(character: playerMemento.Character.ReplacePosition(playerPosition.Value));

            EntityManager = new EntityManager(map.Entities, playerMemento, partyMembers, playerPosition.Value, resetPertyPositions, receiver, gameManager, this);

            _dungeonData = data;

            if (map.MonsterHouse.HasValue)
            {
                _monsterHouse = new MonsterHouse(map.MonsterHouse.Value, EntityManager.Player.Character.Entity.CurrentPosition);
                _rooms.Add(_monsterHouse);
            }

            if (map.Shop.HasValue)
            {
                var clerk = EntityManager.Characters.FirstOrDefault(character =>
                    character.Entity.Id == map.Shop.Value.ClerkId);
                if (clerk == null && !map.Shop.Value.IsStolen)
                {
                    var clerkPosition = GetAllBlankAndStandablePositions()
                        .In(map.Shop.Value.Room.Room.RectRange())
                        .GetAtRandom();
                    clerk = EntityManager.SpawnCharacter(
                        CharacterFactory.BuildCharacter(
                            _dungeonData.Clerk,
                            clerkPosition.Position,
                            homeLocation: new Location(Id, clerkPosition.Position)),
                            gameManager,
                            this);
                }

                if (clerk != null)
                {
                    _shop = new Shop(map.Shop.Value, clerk, gameManager, this);
                    _rooms.Add(_shop);
                }
            }

            SetRules(gameManager);

            var visibleArea = EntityManager.Player.Character.VisionRange.VisibleArea;
            _tilemap.UpdateChunk(EntityManager.Player.Character.Entity.CurrentPosition);
            _tilemap.SetTilesKnown(visibleArea, true);

            UpdateVisibility(EntityManager.Entities);

            if (map.MonsterHouse.HasValue && !map.MonsterHouse.Value.HasEverEntered)
            {
                GameLog.AddIgnoreVisibility("<color=yellow>不穏な気配を感じる……</color>");
            }
        }

        public Observable<OnEffectSpawnedMessage> OnEffectSpawned => _onEffectSpawned;

        public void Dispose()
        {
            EntityManager.Dispose();
            _disposables.Dispose();
        }

        public IItemEntity SpawnItem(IItem item, Vector2Int position)
        {
            var itemEntity = EntityManager.SpawnItem(item,
                FindBlankPositionFrom(position, position => At(position).IsBlankAndStandable(EntityLayer.Bottom)));
            ExecuteEntityTouchEventsAt(itemEntity.Entity.CurrentPosition, itemEntity).Forget();
            return itemEntity;
        }

        public void SpawnEnemy(EnemyData enemy, Vector2Int position, bool doActImmediately, IAffiliation? affiliation = null,
            bool? isSlept = null, bool? isShiny = null)
        {
            if (enemy.CanMimic)
            {
                switch (enemy.MimicWeights.GetRandomIndex())
                {
                    case 0:
                        SpawnMimicItemRevealOnPickup(enemy, position);
                        break;
                    case 1:
                        SpawnMimicItemRevealOnUse(enemy, position);
                        break;
                    case 2:
                        SpawnMimicMoney(enemy, position);
                        break;
                    case 3:
                        SpawnMimicStairs(enemy, position);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                SpawnEnemyIgnoreMimic(enemy, position, doActImmediately, affiliation, isSlept, isShiny);
            }
        }

        public ICharacter SpawnEnemyIgnoreMimic(EnemyData enemy, Vector2Int position, bool doActImmediately, IAffiliation? affiliation = null,
            bool? isSlept = null, bool? isShiny = null)
        {
            return EntityManager.SpawnCharacter(
                CharacterFactory.BuildCharacter(
                    enemy,
                    FindBlankPositionFrom(position, position => At(position).IsBlankAndStandable(EntityLayer.Middle)),
                    isSlept: isSlept ?? RandUtils.IsLessThanProbability(_dungeonData.SleepChance),
                    isShiny: isShiny ?? false,
                    affiliation: affiliation,
                    doActImmediately: doActImmediately
                ),
                _gameManager,
                this
            );
        }

        public void SpawnMimicItemRevealOnPickup(EnemyData enemy, Vector2Int position)
        {
            var dummyItem = _dungeonData.ItemDatabase.GetRandomItem(_dungeonData.Progress).Build();
            EntityManager.SpawnMimicItem(MimicItemEntity.Build(ItemEntity.Build(position, dummyItem), enemy));
        }

        public void SpawnMimicItemRevealOnUse(EnemyData enemy, Vector2Int position)
        {
            var category = RandUtils.WeightedIndex(1, 1, 1, 1, 1) switch
            {
                0 => ItemCategory.Potions,
                1 => ItemCategory.Scrolls,
                2 => ItemCategory.Books,
                3 => ItemCategory.Wands,
                4 => ItemCategory.Weapons,
                _ => throw new NotImplementedException()
            };
            var dummyItem = _dungeonData.ItemDatabase.GetRandomItem(category, _dungeonData.Progress).Build(mimic: enemy);
            var itemEntity = EntityManager.SpawnItemFromMemento(ItemEntity.Build(position, dummyItem));
            ExecuteEntityTouchEventsAt(itemEntity.Entity.CurrentPosition, itemEntity).Forget();
        }

        public void SpawnMimicMoney(EnemyData enemy, Vector2Int position)
        {
            EntityManager.SpawnMimicMoney(MimicMoney.Build(position, _dungeonData.MoneyAmount(), enemy));
        }

        public void SpawnMimicStairs(EnemyData enemy, Vector2Int position)
        {
            EntityManager.SpawnMimicStairs(MimicStairs.Build(MovementEntityType.DownStairs, position, enemy));
        }

        public bool SpawnRandomEnemy(Vector2Int position, bool? isSlept = null)
        {
            if (_dungeonData.Enemies.Count == 0)
                return false;
            SpawnEnemy(
                _dungeonData.Enemies.GetRandomItem(),
                position,
                doActImmediately: false,
                isSlept: isSlept
            );
            return true;
        }

        public ICharacter? SpawnRandomEnemyIgnoreMimic(Vector2Int position, bool? isSlept = null)
        {
            if (_dungeonData.Enemies.Count == 0)
                return null;
            return SpawnEnemyIgnoreMimic(
                _dungeonData.Enemies.GetRandomItem(),
                position,
                doActImmediately: false,
                isSlept: isSlept
            );
        }

        public async UniTask<Vector2Int> ShowThrowAnimation(Sprite icon, Vector2Int position, Direction8 direction,
            int distance, bool isPiercing, params EntityLayer[] canHitLayer)
        {
            return await EntityManager.ShowThrowAnimation(icon, position, direction, distance, isPiercing, this, canHitLayer);
        }

        public void SpawnEffect(IEnumerable<Vector2Int> area, Color color)
        {
            _onEffectSpawned.OnNext(new OnEffectSpawnedMessage(area, color));
        }

        public IMapPosition At(Vector2Int position)
        {
            return new MapPosition(position, this, TilemapViewer);
        }

        public void UpdateVisibility(IEnumerable<IEntity> entities)
        {
            foreach (var entity in entities)
                UpdateVisibility(entity);
        }

        public void UpdateVisibility(IEntity entity)
        {
            bool visibility;
            if (IsGrass(entity.Entity.CurrentPosition) &&
                (entity.Entity.Layer == EntityLayer.Bottom || entity.Entity.Layer == EntityLayer.Floor))
                visibility = false;
            else
                visibility = EntityManager.Player.Character.IsVisible(entity.Entity.CurrentPosition);
            entity.Entity.SetVisibility(visibility);
        }

        public bool IsGrass(Vector2Int position)
        {
            return TilemapViewer.IsGrass(position);
        }

        public async UniTask ExecuteEntityTouchEventsAt(Vector2Int position, IEntity triggerEntity)
        {
            foreach (var entityEventEntity in EntityManager.GetEntityEventEntitiesFastAt(position,
                         EntityLayer.Floor, EntityLayer.Bottom))
                await entityEventEntity.Event.DoEvent(triggerEntity, _gameManager, this);
        }

        public async UniTask ExecuteCharacterTouchEventsAt(Vector2Int position, ICharacter character)
        {
            foreach (var characterEventEntity in EntityManager.GetCharacterEventEntitiesFastAt(position,
                         EntityLayer.Floor, EntityLayer.Bottom))
                await characterEventEntity.Event.DoEvent(character, _gameManager, this);
        }

        public bool IsInside(Vector2Int position)
        {
            return _tilemap.IsPositionInsideMap(position);
        }

        public HashSet<Vector2Int> GetAllPositions()
        {
            return _tilemap.GetAllTiles().Select(tile => tile.position).ToHashSet();
        }

        public IEnumerable<IMapPosition> GetAllBlankPositions() => GetAllBlankPositionsOn();
        public IEnumerable<IMapPosition> GetAllBlankPositionsOn(params EntityLayer[] layers)
        {
            return TilemapViewer
                .GetAllPassablePositions()
                .Select(position => At(position))
                .Where(position => position.IsBlank(layers));
        }

        public IEnumerable<IMapPosition> GetAllBlankAndStandablePositions() => GetAllBlankAndStandablePositionsOn();
        public IEnumerable<IMapPosition> GetAllBlankAndStandablePositionsOn(params EntityLayer[] layers)
        {
            return TilemapViewer
                .GetAllWalkablePositions()
                .Select(position => At(position))
                .Where(position => position.IsBlank(layers));
        }

        public IEnumerable<IMapPosition> GetAllWalkablePositions(IAffiliation affiliation)
        {
            var result = TilemapViewer.GetAllWalkablePositions();
            result.ExceptWith(
                EntityManager.Entities
                    .On(EntityLayer.Middle)
                    .Where(entity => !(entity is ICharacter character && !character.Affiliation.IsEnemy(affiliation)))
                    .Positions());
            return result.Select(position => At(position));
        }

        public bool IsReachable(Vector2Int from, Vector2Int to, IHasBehavior actor)
        {
            var calculator = new MoveCostCalculator(actor, this, true);
            var route = new AStar(calculator.Calculate).Calc(from, to);
            if (!route.Any())
                return false;
            if (At(to).IsWalkable(actor.Affiliation))
                return route.Last() == to;
            return (route.Last() - to).sqrMagnitude <= 2;
        }

        public ITilemapViewer TilemapViewer => _tilemap;

        public MapMemento Serialize()
        {
            var characters = EntityManager.Characters.ToList();
            characters.Remove(EntityManager.Player.Character);
            return new MapMemento
            (
                Id,
                _tilemap.Serialize(),
                EntityManager.Serialize(),
                _monsterHouse.ToOption().Map(x => x.Serialize()),
                _shop.ToOption().Map(x => x.Serialize()),
                GetAllBlankAndStandablePositions().GetAtRandom().Position
            );
        }

        public MapMemento SerializeWithoutPartyMembers()
        {
            return new MapMemento
            (
                Id,
                _tilemap.Serialize(),
                EntityManager.SerializeWithoutPartyMembers(GetFollowingCharacters()),
                _monsterHouse.ToOption().Map(x => x.Serialize()),
                _shop.ToOption().Map(x => x.Serialize()),
                GetAllBlankAndStandablePositions().GetAtRandom().Position
            );
        }

        private void SetRules(IGameManager gameManager)
        {
            EntityManager.SetRules();

            EntityManager.Characters.SubscribeIncludingCurrentObservables(
                character => character.OnDead,
                (character, _) =>
                {
                    if (!character.IsPlayer)
                        DropAllItem(character);
                }
            ).AddTo(_disposables);

            EntityManager.Player.Character.VisionRange.OnVisibleAreaChanged.Subscribe(areaChanged =>
            {
                _tilemap.SetTilesKnown(EntityManager.Player.Character.VisionRange.VisibleArea, true);
                UpdateVisibility(EntityManager.Entities);
            }).AddTo(_disposables);

            EntityManager.Player.Character.Entity.Position.Subscribe(async positionChanged =>
            {
                _tilemap.UpdateChunk(positionChanged);
                if (IsGrass(positionChanged))
                {
                    Log.Debug($"SetGrasses: {EntityManager.Player.Character.Entity.CurrentPosition}");
                    SetGrasses(new[] { EntityManager.Player.Character.Entity.CurrentPosition }, false);
                    _gameManager.PlaySE(SE.GrassWalk);
                }
                var eventId = gameManager.StartEvent();
                foreach (var eventArea in _rooms)
                {
                    await eventArea.UpdatePosition(_gameManager, this, positionChanged);
                }
                gameManager.EndEvent(eventId);

            }).AddTo(_disposables);

            EntityManager.Characters.SubscribeIncludingCurrentObservables(
                character => character.Entity.Position.SkipLatestValueOnSubscribe(),
                async (character, positionChanged) =>
                {
                    var item = EntityManager.GetItemAt(positionChanged);
                    if (item != null)
                    {
                        Log.Debug($"CanPickUp: {character.CanPickUp}");
                        Log.Debug($"CanPickUpItem: {character.CanPickUpItem()}");
                        Log.Debug($"CanPickUpAt: {EntityManager.CanPickUpAt(positionChanged, character.IsPlayer && Settings.GlobalSettings.AutoPickUpShopItem.CurrentValue)}");
                        if (character.CanPickUp
                            && character.CanPickUpItem()
                            && EntityManager.CanPickUpAt(positionChanged,
                                character.IsPlayer && Settings.GlobalSettings.AutoPickUpShopItem.CurrentValue))
                        {
                            EntityManager.PickUpAt(positionChanged,
                                character.IsPlayer && Settings.GlobalSettings.AutoPickUpShopItem.CurrentValue);
                            if (character.Inventory.CanAddToEmpty())
                            {
                                character.Inventory.AddToEmpty(item.Item);
                                _gameManager.PlaySE(SE.Pickup);
                                _gameManager.RequestWorldIconPopup(item.Icon, positionChanged);
                                if (EntityManager.Player.Character.IsVisible(positionChanged))
                                {
                                    GameLog.Add(character.Entity.IsVisible,
                                        $"{character.GetName(EntityManager.Player)}は<color=yellow>{item.Item.GetName(EntityManager.Player, ItemPlaceholders)}</color>を拾った");
                                }
                            }
                            else
                            {
                                throw new Exception("Unexpected error. Unable to pick up item.");
                            }
                        }
                        else if (EntityManager.Player.Character.IsVisible(positionChanged))
                        {
                            GameLog.Add(character.Entity.IsVisible,
                                $"{character.GetName(EntityManager.Player)}は<color=yellow>{item.Item.GetName(EntityManager.Player, ItemPlaceholders)}</color>の上に乗った");
                        }
                    }
                }
            ).AddTo(_disposables);

            EntityManager.Entities.SubscribeIncludingCurrentObservables(
                entity => entity.Entity.Position.SkipLatestValueOnSubscribe(),
                async (entity, positionChanged) =>
                {
                    if (entity.Entity.IsVisualOnly.CurrentValue)
                        return;
                    var eventId = gameManager.StartEvent();
                    await ExecuteEntityTouchEventsAt(positionChanged, entity);
                    gameManager.EndEvent(eventId);
                }
            ).AddTo(_disposables);

            EntityManager.Characters.SubscribeIncludingCurrentObservables(
                character => character.Entity.Position.SkipLatestValueOnSubscribe(),
                async (character, positionChanged) =>
                {
                    var eventId = gameManager.StartEvent();
                    await ExecuteCharacterTouchEventsAt(positionChanged, character);
                    if (character.IsPlayer)
                    {
                        foreach (var playerEventEntity in EntityManager.GetPlayerEventEntitiesFastAt(positionChanged,
                                     EntityLayer.Floor, EntityLayer.Bottom))
                        {
                            await playerEventEntity.Events.DoEvent(EntityManager.Player, _gameManager, this);
                        }
                    }

                    gameManager.EndEvent(eventId);
                }
            ).AddTo(_disposables);

            EntityManager.Characters.SubscribeIncludingCurrentObservables(
                character => character.Status.GetFlagProperty(FlagStatType.IsAffectedByTrap).SkipLatestValueOnSubscribe(),
                async (character, affectedByTrap) =>
                {
                    var eventId = gameManager.StartEvent();
                    if (affectedByTrap && !character.IsGrounded
                        && EntityManager.GetEntityEventEntityFastAt(character.Entity.CurrentPosition, EntityLayer.Floor)
                            is Trap trap)
                    {
                        await trap.Event.DoEvent(character, _gameManager, this);
                    }
                    gameManager.EndEvent(eventId);
                }
            ).AddTo(_disposables);

            EntityManager.Entities.SubscribeIncludingCurrentObservables(
                entity => entity.Entity.Position,
                (entity, _) => UpdateVisibility(entity)
            ).AddTo(_disposables);

            _tilemap.OnOverlayTilesChanged.Subscribe(overlayTilesChanged =>
            {
                foreach (var (position, category) in overlayTilesChanged)
                {
                    var entity = EntityManager.Entities.At(position);
                    foreach (var e in entity)
                    {
                        if (e.Entity.Layer == EntityLayer.Bottom || e.Entity.Layer == EntityLayer.Floor)
                            UpdateVisibility(e);
                    }
                }
            }).AddTo(_disposables);

            _tilemap.OnTilesChanged.Subscribe(tileChanged =>
            {
                _fullVisibleArea = null;
                foreach (var (position, _) in tileChanged)
                {
                    var visibleArea = GetVisibleArea(position);
                    foreach (var pos in visibleArea)
                    {
                        _visionCache.Remove(pos);
                    }

                    _tilemap.SetTilesKnown(visibleArea, true);
                    EntityManager.Characters.In(visibleArea).ForEach(character => character.VisionRange.Refresh());
                }
            }).AddTo(_disposables);
        }

        public async UniTask UpdateTurn(int turn)
        {
            if (RandUtils.IsLessThanProbability(CommonSenseParameters.SpawnEnemyProbabilityPerTurn))
            {
                var positions = GetAllBlankAndStandablePositions().Values()
                    .Except(EntityManager.Player.Character.VisionRange.VisibleArea);
                if (positions.Any())
                    SpawnRandomEnemy(positions.GetAtRandom());
            }

            var unloadedCharacters = EntityManager.Characters
                .Where(character => !_tilemap.IsPositionInsideActiveChunk(character.Entity.CurrentPosition))
                .ToList();
            foreach (var character in unloadedCharacters)
            {
                EntityManager.RemoveCharacter(character);
            }

            await EntityManager.UpdateTurn(_gameManager, this);

            SetGrasses(EntityManager.FireEntities.Positions(), false);

            _tilemap.UpdateTurn();
        }

        public void RemoveWalls(IEnumerable<Vector2Int> positions)
        {
            _tilemap.RemoveWalls(positions);
        }

        public void SetGrasses(IEnumerable<Vector2Int> positions, bool isGrass)
        {
            _tilemap.SetOverlayTiles(positions, isGrass ? OverlayTileCategory.Grass : null);
        }

        public void SetIce(IEnumerable<Vector2Int> positions, bool isIce)
        {
            _tilemap.SetOverlayTiles(positions, isIce ? OverlayTileCategory.FloatingIce : null);
        }

        public void DropAllItem(ICharacter character)
        {
            foreach (var item in character.Inventory.Clear())
            {
                SpawnItem(item,
                    FindBlankPositionFrom(character.Entity.CurrentPosition,
                        position => At(position).IsBlankAndStandable(EntityLayer.Bottom)));
            }
        }

        public Vector2Int FindBlankPositionFrom(Vector2Int position, Func<Vector2Int, bool> isBlankFunc)
        {
            return BlankFinder.FindBlankPosition(isBlankFunc, TilemapViewer.IsWalkable, position);
        }

        public Vector2Int GetThrowDestination(Vector2Int position, Direction8 direction, int distance, params EntityLayer[] canHitLayer)
        {
            var pos = position;
            for (var i = 0; i < distance; i++)
            {
                if (At(pos + direction.Vector()).IsBlank(canHitLayer))
                {
                    pos += direction.Vector();
                }
                else if (At(pos + direction.Vector()).IsPassableOnMap())
                {
                    pos += direction.Vector();
                    break;
                }
                else
                {
                    break;
                }
            }

            return pos;
        }

        public IEnumerable<Vector2Int> GetThrowDestinationPiercing(Vector2Int position, Direction8 direction, int distance, params EntityLayer[] canHitLayer)
        {
            var pos = position;
            for (var i = 0; i < distance; i++)
            {
                if (At(pos + direction.Vector()).IsBlank(canHitLayer))
                {
                    pos += direction.Vector();
                }
                else if (At(pos + direction.Vector()).IsPassableOnMap())
                {
                    pos += direction.Vector();
                    yield return pos;
                }
                else
                {
                    break;
                }
            }

            yield return pos;
        }

        private Dictionary<Vector2Int, HashSet<Vector2Int>> _visionCache = new();
        private HashSet<Vector2Int>? _fullVisibleArea;

        public bool IsVisible(Vector2Int from, Vector2Int to, float radius)
        {
            if ((from - to).sqrMagnitude > radius * radius)
                return false;
            if (_visionCache.TryGetValue(from, out var area))
                return area.Contains(to);
            if (_visionCache.TryGetValue(to, out area))
                return area.Contains(from);
            UpdateVisibleAreaCache(from);
            return _visionCache[from].Contains(to);
        }

        public HashSet<Vector2Int> GetVisibleArea(Vector2Int from, float radius)
        {
            return GetVisibleArea(from).Where(x => (x - from).sqrMagnitude <= radius * radius).ToHashSet();
        }

        public HashSet<Vector2Int> GetVisibleArea(Vector2Int from)
        {
            if (_visionCache.TryGetValue(from, out var area))
                return area;
            UpdateVisibleAreaCache(from);
            return _visionCache[from];
        }

        public HashSet<Vector2Int> GetFullVisibleArea()
        {
            if (_fullVisibleArea == null)
                _fullVisibleArea = ViewCalculator.ComputeFullVisibility(_tilemap.GetAllLightPassablePositions());
            return _fullVisibleArea;
        }

        private void UpdateVisibleAreaCache(Vector2Int from)
        {
            _visionCache[from] = ViewCalculator.FieldOfView(from, 20, pos => !At(pos).IsLightPassable());
        }

        public HashSet<Vector2Int> ComputeCircle(Func<Vector2Int, bool> isTileBlocked, Vector2Int position,
            float radius)
        {
            var viewRadiusSq = radius * radius;
            var viewArea = ViewCalculator.FieldOfView(position, Mathf.CeilToInt(radius), isTileBlocked);
            return viewArea.Where(x => (x - position).sqrMagnitude <= viewRadiusSq).ToHashSet();
        }

        public IPlayer Player => EntityManager?.Player;
        public IObservableCollection<IEntity> Entities => EntityManager.Entities;
        public IObservableCollection<ICharacter> Characters => EntityManager.Characters;
        public IObservableCollection<IItemEntity> Items => EntityManager.Items;
        public IObservableCollection<IEntityEventEntity> StandaloneEntityEventEntities => EntityManager.StandaloneEntityEventEntities;
        public IObservableCollection<ICharacterEventEntity> StandaloneCharacterEventEntities => EntityManager.StandaloneCharacterEventEntities;
        public IObservableCollection<IPlayerEventEntity> StandalonePlayerEventEntities => EntityManager.StandalonePlayerEventEntities;
        public IObservableCollection<IScheduledEventEntity> StandaloneScheduledEventEntities => EntityManager.StandaloneScheduledEventEntities;
        public IObservableCollection<IPlayerEventEntity> PlayerEventEntities => EntityManager.PlayerEventEntities;
        public IObservableCollection<IScheduledEventEntity> ScheduledEventEntities => EntityManager.ScheduledEventEntities;
        public IObservableCollection<ThrowAnimationEntity> ThrowAnimationEntities => EntityManager.ThrowAnimationEntities;
        public IObservableCollection<Fire> FireEntities => EntityManager.FireEntities;
        public List<Stairs> Stairs => EntityManager.Stairs;
        public IEnumerable<ILockedEntity> LockedEntities => EntityManager.LockedEntities;
        public IEntity? GetEntityFastAt(Vector2Int position, EntityLayer layer) => EntityManager.GetEntityFastAt(position, layer);
        public IEnumerable<IEntity> GetEntitiesFastAt(Vector2Int position, IEnumerable<EntityLayer> layers) => EntityManager.GetEntitiesFastAt(position, layers);
        public IEnumerable<IEntity> GetEntitiesFastAt(Vector2Int position, params EntityLayer[] layers) => EntityManager.GetEntitiesFastAt(position, layers);
        public IEnumerable<IEntity> GetEntitiesFastAt(Vector2Int position) => EntityManager.GetEntitiesFastAt(position);
        public IEntityEventEntity? GetEntityEventEntityFastAt(Vector2Int position, EntityLayer layer) => EntityManager.GetEntityEventEntityFastAt(position, layer);
        public IEnumerable<IEntityEventEntity> GetEntityEventEntitiesFastAt(Vector2Int position, IEnumerable<EntityLayer> layers) => EntityManager.GetEntityEventEntitiesFastAt(position, layers);
        public IEnumerable<IEntityEventEntity> GetEntityEventEntitiesFastAt(Vector2Int position, params EntityLayer[] layers) => EntityManager.GetEntityEventEntitiesFastAt(position, layers);
        public ICharacterEventEntity? GetCharacterEventEntityFastAt(Vector2Int position, EntityLayer layer) => EntityManager.GetCharacterEventEntityFastAt(position, layer);
        public IEnumerable<ICharacterEventEntity> GetCharacterEventEntitiesFastAt(Vector2Int position, IEnumerable<EntityLayer> layers) => EntityManager.GetCharacterEventEntitiesFastAt(position, layers);
        public IEnumerable<ICharacterEventEntity> GetCharacterEventEntitiesFastAt(Vector2Int position, params EntityLayer[] layers) => EntityManager.GetCharacterEventEntitiesFastAt(position, layers);
        public IPlayerEventEntity? GetPlayerEventEntityFastAt(Vector2Int position, EntityLayer layer) => EntityManager.GetPlayerEventEntityFastAt(position, layer);
        public IEnumerable<IPlayerEventEntity> GetPlayerEventEntitiesFastAt(Vector2Int position, IEnumerable<EntityLayer> layers) => EntityManager.GetPlayerEventEntitiesFastAt(position, layers);
        public IEnumerable<IPlayerEventEntity> GetPlayerEventEntitiesFastAt(Vector2Int position, params EntityLayer[] layers) => EntityManager.GetPlayerEventEntitiesFastAt(position, layers);
        public IScheduledEventEntity? GetScheduledEventEntityFastAt(Vector2Int position, EntityLayer layer) => EntityManager.GetScheduledEventEntityFastAt(position, layer);
        public IEnumerable<IScheduledEventEntity> GetScheduledEventEntitiesFastAt(Vector2Int position, IEnumerable<EntityLayer> layers) => EntityManager.GetScheduledEventEntitiesFastAt(position, layers);
        public IEnumerable<IScheduledEventEntity> GetScheduledEventEntitiesFastAt(Vector2Int position, params EntityLayer[] layers) => EntityManager.GetScheduledEventEntitiesFastAt(position, layers);
        public IItem? GetItemByIdFromWorldOrInventory(Id<IItem> id) => EntityManager.GetItemByIdFromWorldOrInventory(id);
        public HashSet<Vector2Int> AllCharacterPositionsFast() => EntityManager.AllCharacterPositionsFast();
        public HashSet<Vector2Int> AllItemPositionsFast() => EntityManager.AllItemPositionsFast();
        public void RevealMimic(IEnumerable<Vector2Int> positions) => EntityManager.RevealMimic(positions);
        public void AttackStatue(IEnumerable<Vector2Int> positions) => EntityManager.AttackStatue(positions);
        public void SpawnFire(IEnumerable<Vector2Int> positions)
        {
            foreach (var position in positions)
            {
                if (At(position).CanPlace(false, false, true))
                    EntityManager.SpawnFire(position);
            }
        }
        public IItemEntity? TryPickUpAt(Vector2Int position, bool canPickUpShopItem)
        {
            _gameManager.PlaySE(SE.Pickup);
            var item = EntityManager.TryPickUpAt(position, canPickUpShopItem);
            if (item != null)
            {
                _gameManager.RequestWorldIconPopup(item.Icon, position);
            }
            return item;
        }
        public IEnumerable<ICharacter> GetFollowingCharacters() => EntityManager.GetFollowingCharacters();
    }
}