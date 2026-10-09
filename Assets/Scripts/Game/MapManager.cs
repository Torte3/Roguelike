#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Configuration;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Evaluation;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.WorldEvents;
using Domain.Service.Characters;
using Domain.Service.Characters.Behavior;
using Domain.Service.Events;
using Domain.Service.Items;
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
        public Id<IMap> Id { get; }
        public string Name => "Dungeon";
        public int Depth { get; }
        public MapType Type => _floorSpec.Type;
        public ItemDatabase ItemDatabase => _floorSpec.ItemDatabase;
        public ItemPlaceholders ItemPlaceholders { get; }
        IReadOnlyItemPlaceholders IReadOnlyMap.ItemPlaceholders => ItemPlaceholders;
        public IWorldEventRecorder Events { get; }
        public ItemMarketPriceTable MarketPriceTable { get; }
        private readonly CompositeDisposable _disposables = new();
        private readonly ITilemap _tilemap;
        private readonly FloorSpec _floorSpec;
        private readonly float _progress;
        private readonly List<IEventArea> _rooms;
        private readonly MonsterHouse? _monsterHouse;
        private readonly Shop? _shop;
        public IReadOnlyShop? Shop => _shop;
        private readonly IGameManager _gameManager;
        private EntityManager EntityManager { get; }

        public MapManager(MapMemento map, FloorSpec spec, int depth, float progress, PlayerData playerData, IGameManager gameManager, CharacterControlInputReceiver receiver, ItemPlaceholders itemPlaceholders, ItemMarketPriceTable marketPriceTable, IWorldEventRecorder events)
        {
            Id = map.Id;
            Depth = depth;
            _floorSpec = spec;
            _progress = progress;
            ItemPlaceholders = itemPlaceholders;
            Events = events;
            MarketPriceTable = marketPriceTable;
            _gameManager = gameManager;

            var playerPosition = map.InitialPlayerPosition;

            _tilemap = new Tilemap(map.Tilemap, events);

            var playerMemento = CharacterFactory.BuildPlayer(playerData, playerPosition);

            EntityManager = new EntityManager(map.Entities, playerMemento, new(), playerPosition, false, receiver, gameManager, this);

            (_monsterHouse, _shop, _rooms) = BuildRooms(map);
            ApplyInitialMapState(map, gameManager, true);
        }

        public MapManager(MapMemento map, FloorSpec spec, int depth, float progress, PlayerMemento playerMemento,
            List<CharacterMemento> partyMembers,
            Vector2Int? playerPosition, bool resetPertyPositions, IGameManager gameManager, CharacterControlInputReceiver receiver, ItemPlaceholders itemPlaceholders, ItemMarketPriceTable marketPriceTable, IWorldEventRecorder events, bool isNewWorld)
        {
            Id = map.Id;
            Depth = depth;
            _floorSpec = spec;
            _progress = progress;
            ItemPlaceholders = itemPlaceholders;
            Events = events;
            MarketPriceTable = marketPriceTable;
            _gameManager = gameManager;

            if (playerPosition == null)
            {
                playerPosition = map.InitialPlayerPosition;
            }

            _tilemap = new Tilemap(map.Tilemap, events);

            playerMemento = playerMemento.CopyWith(character: playerMemento.Character.ReplacePosition(playerPosition.Value));

            EntityManager = new EntityManager(map.Entities, playerMemento, partyMembers, playerPosition.Value, resetPertyPositions, receiver, gameManager, this);

            (_monsterHouse, _shop, _rooms) = BuildRooms(map);
            ApplyInitialMapState(map, gameManager, isNewWorld);
        }

        private (MonsterHouse? MonsterHouse, Shop? Shop, List<IEventArea> Rooms) BuildRooms(MapMemento map)
        {
            var rooms = new List<IEventArea>();

            MonsterHouse? monsterHouse = null;
            if (map.MonsterHouse.HasValue)
            {
                monsterHouse = new MonsterHouse(map.MonsterHouse.Value, EntityManager.Player.Character.Entity.CurrentPosition);
                rooms.Add(monsterHouse);
            }

            Shop? shop = null;
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
                            _floorSpec.Clerk,
                            clerkPosition.Position,
                            homeLocation: new Location(Id, clerkPosition.Position)),
                            this);
                }

                if (clerk != null)
                {
                    shop = new Shop(map.Shop.Value, clerk, this);
                    rooms.Add(shop);
                }
            }

            return (monsterHouse, shop, rooms);
        }

        private void ApplyInitialMapState(MapMemento map, IGameManager gameManager, bool isNewWorld)
        {
            var player = EntityManager.Player;
            var visibleArea = player.Character.VisionRange.VisibleArea;
            Events.Record(new MapEntered(
                Name,
                Depth,
                Type,
                _tilemap.Rect,
                _shop?.Rect,
                _shop?.IsInside.CurrentValue == true,
                _shop?.IsStolen == true,
                player.Character.Entity.CurrentPosition,
                player.Money,
                _tilemap.GetAllTiles().Select(tile => tile.tileData.ToState(tile.position)).ToList(),
                _tilemap.GetAllOverlayTiles().ToList(),
                visibleArea,
                isNewWorld,
                player.Character.WholeInventoryLookIn(this),
                this.PlayerUnderfoot(), this.ShopLookIn()));

            if (map.MonsterHouse.HasValue && !map.MonsterHouse.Value.HasEverEntered)
            {
                Events.Record(new OminousPresenceFelt());
            }

            SetRules(gameManager);

            _tilemap.SetTilesKnown(visibleArea, true);
        }

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
                    isSlept: isSlept ?? RandUtils.IsLessThanProbability(_floorSpec.SleepChance),
                    isShiny: isShiny ?? false,
                    affiliation: affiliation,
                    doActImmediately: doActImmediately
                ),
                this
            );
        }

        private void SpawnMimicItemRevealOnPickup(EnemyData enemy, Vector2Int position)
        {
            var dummyItem = _floorSpec.ItemDatabase.GetRandomItem(_progress).Build();
            EntityManager.SpawnMimicItem(MimicItemEntity.Build(ItemEntity.Build(position, dummyItem), enemy));
        }

        private void SpawnMimicItemRevealOnUse(EnemyData enemy, Vector2Int position)
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
            var dummyItem = _floorSpec.ItemDatabase.GetRandomItem(category, _progress).Build(mimic: enemy);
            var itemEntity = EntityManager.SpawnItemFromMemento(ItemEntity.Build(position, dummyItem));
            ExecuteEntityTouchEventsAt(itemEntity.Entity.CurrentPosition, itemEntity).Forget();
        }

        private void SpawnMimicMoney(EnemyData enemy, Vector2Int position)
        {
            EntityManager.SpawnMimicMoney(MimicMoney.Build(position, _floorSpec.MoneyAmount(), enemy));
        }

        private void SpawnMimicStairs(EnemyData enemy, Vector2Int position)
        {
            EntityManager.SpawnMimicStairs(MimicStairs.Build(MovementEntityType.DownStairs, position, enemy));
        }

        public bool SpawnRandomEnemy(Vector2Int position, bool? isSlept = null)
        {
            if (_floorSpec.Enemies.Count == 0)
                return false;
            SpawnEnemy(
                _floorSpec.Enemies.GetRandomItem(),
                position,
                doActImmediately: false,
                isSlept: isSlept
            );
            return true;
        }

        public ICharacter? SpawnRandomEnemyIgnoreMimic(Vector2Int position, bool? isSlept = null)
        {
            if (_floorSpec.Enemies.Count == 0)
                return null;
            return SpawnEnemyIgnoreMimic(
                _floorSpec.Enemies.GetRandomItem(),
                position,
                doActImmediately: false,
                isSlept: isSlept
            );
        }

        public IMapPosition At(Vector2Int position)
        {
            return At(position, _tilemap);
        }

        public IMapPosition At(Vector2Int position, ITerrain terrain)
        {
            return new MapPosition(position, this, terrain);
        }

        private void UpdateVisibility(IEnumerable<IEntity> entities)
        {
            foreach (var entity in entities)
                UpdateVisibility(entity);
        }

        private void UpdateVisibility(IEntity entity)
        {
            entity.Entity.SetVisibility(IsVisibleToPlayer(entity.Entity, entity.Entity.CurrentPosition));
        }

        private bool IsVisibleToPlayer(EntityBase entity, Vector2Int position)
        {
            if (IsGrass(position) &&
                !entity.IgnoreGrass &&
                (entity.Layer == EntityLayer.Bottom || entity.Layer == EntityLayer.Floor))
                return false;
            return EntityManager.Player.Character.IsVisible(position);
        }

        public bool IsGrass(Vector2Int position)
        {
            return _tilemap.IsGrass(position);
        }

        // キャラがアイテムの上に乗ったとき、拾える条件を満たせば自動で拾う。拾えない場合は「乗った」ログのみ。
        private void TryAutoPickUpOnMove(ICharacter character, Vector2Int positionChanged)
        {
            var item = EntityManager.GetItemAt(positionChanged);
            if (item == null)
                return;

            var autoPickUpShopItem = character.IsPlayer && Settings.GlobalSettings.AutoPickUpShopItem.CurrentValue;
            var canPickUp = character.CanPickUp
                            && character.CanPickUpItem()
                            && EntityManager.CanPickUpAt(positionChanged, autoPickUpShopItem);
            if (!canPickUp)
            {
                Events.Record(new ItemSteppedOn(character.Entity.Ref, character.Label, item.Item.NameIn(this)));
                return;
            }

            EntityManager.PickUpAt(positionChanged, autoPickUpShopItem);
            if (!character.Inventory.CanAddToEmpty())
                throw new Exception("Unexpected error. Unable to pick up item.");

            character.Inventory.AddToEmpty(item.Item);
            Events.Record(new ItemPickedUp(character.Entity.Ref, character.Label, item.Item.NameIn(this), true,
                new ObtainedItem(item.Item.Icon, positionChanged), character.HeldItemsIn(this),
                character.UnderfootAfterMove(this, positionChanged)));
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
            return _tilemap
                .GetAllPassablePositions()
                .Select(position => At(position))
                .Where(position => position.IsBlank(layers));
        }

        public IEnumerable<IMapPosition> GetAllBlankAndStandablePositions() => GetAllBlankAndStandablePositionsOn();
        public IEnumerable<IMapPosition> GetAllBlankAndStandablePositionsOn(params EntityLayer[] layers)
        {
            return _tilemap
                .GetAllWalkablePositions()
                .Select(position => At(position))
                .Where(position => position.IsBlank(layers));
        }

        public IEnumerable<IMapPosition> GetAllWalkablePositions(IAffiliation affiliation)
        {
            var result = _tilemap.GetAllWalkablePositions();
            result.ExceptWith(
                EntityManager.Entities
                    .On(EntityLayer.Middle)
                    .Where(entity => !(entity is ICharacter character && !character.Affiliation.IsEnemy(affiliation)))
                    .Positions());
            return result.Select(position => At(position));
        }

        public bool IsReachable(Vector2Int to, IHasBehavior actor)
        {
            var route = actor.RouteTo(to, this);
            if (At(to).IsWalkable(actor.Affiliation))
                return route.Last == to;
            return (route.Last - to).sqrMagnitude <= 2;
        }

        public MapMemento Serialize()
        {
            return new MapMemento
            (
                Id,
                _tilemap.Serialize(),
                EntityManager.Serialize(),
                _monsterHouse.ToOption().Map(x => x.Serialize()),
                _shop.ToOption().Map(x => x.Serialize()),
                // InitialPlayerPosition は新ゲームの初期スポーン用で、生成時(MapBuilder)にだけ埋める。
                // 2回目以降のセーブでは読まれないので空(zero)にしておく。
                Vector2Int.zero
            );
        }

        internal MapMemento SerializeWithoutPartyMembers()
        {
            return new MapMemento
            (
                Id,
                _tilemap.Serialize(),
                EntityManager.SerializeWithoutPartyMembers(GetFollowingCharacters()),
                _monsterHouse.ToOption().Map(x => x.Serialize()),
                _shop.ToOption().Map(x => x.Serialize()),
                // InitialPlayerPosition は新ゲームの初期スポーン用で、生成時(MapBuilder)にだけ埋める。
                // 2回目以降のセーブでは読まれないので空(zero)にしておく。
                Vector2Int.zero
            );
        }

        private void RecordShopRoomItemChanges()
        {
            if (_shop == null)
                return;

            var shop = _shop;
            var itemsInRoom = EntityManager.Items.Where(item => shop.Rect.Contains(item.Entity.CurrentPosition)).ToHashSet();

            void Update(IItemEntity item, bool isOnMap)
            {
                var isInRoom = isOnMap && shop.Rect.Contains(item.Entity.CurrentPosition);
                if (isInRoom ? itemsInRoom.Add(item) : itemsInRoom.Remove(item))
                    Events.Record(new ShopRoomItemsChanged(this.ShopLookIn()));
            }

            EntityManager.Items.ObserveAdd().Subscribe(added => Update(added.Value, true)).AddTo(_disposables);
            EntityManager.Items.ObserveRemove().Subscribe(removed => Update(removed.Value, false)).AddTo(_disposables);
            EntityManager.Items.SubscribeIncludingCurrentObservables(
                item => item.Entity.Position.SkipLatestValueOnSubscribe(),
                (item, _) => Update(item, true)
            ).AddTo(_disposables);
        }

        private void SetRules(IGameManager gameManager)
        {
            EntityManager.SetRules(IsVisibleToPlayer);
            RecordShopRoomItemChanges();

            EntityManager.Player.Character.VisionRange.OnVisibleAreaChanged.Subscribe(areaChanged =>
            {
                var visibleArea = EntityManager.Player.Character.VisionRange.VisibleArea;
                Events.Record(new SightChanged(visibleArea));
                _tilemap.SetTilesKnown(visibleArea, true);
                UpdateVisibility(EntityManager.Entities);
            }).AddTo(_disposables);

            EntityManager.Player.Character.Entity.Position.Subscribe(async positionChanged =>
            {
                if (IsGrass(positionChanged))
                {
                    Events.Record(new GrassTrampled());
                    SetGrasses(new[] { EntityManager.Player.Character.Entity.CurrentPosition }, false);
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
                    TryAutoPickUpOnMove(character, positionChanged);
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
                            // 初めて魔法陣に乗ったとき、移動の選択肢を出す前にチュートリアルを表示する。
                            if (playerEventEntity is IMovementEntity { Type: MovementEntityType.MagicCircle })
                                await _gameManager.ShowTutorialIfNeeded(TutorialType.MagicCircle);
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

            // 壁の破壊などでタイルが変わると、その地点を見通せる範囲の視界キャッシュが古くなる。
            // 変化マスから見えるマスのキャッシュを破棄し、その範囲のキャラの視界を再計算させる。
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

            await EntityManager.UpdateTurn(_gameManager, this);

            SetGrasses(EntityManager.FireEntities.Positions(), false);

            _tilemap.UpdateTurn();
        }

        public void RemoveWalls(IEnumerable<Vector2Int> positions)
        {
            var previousTiles = _tilemap.RemoveWalls(positions);
            foreach (var character in Characters)
                character.RememberTerrainBefore(previousTiles);
        }

        public void SetGrasses(IEnumerable<Vector2Int> positions, bool isGrass)
        {
            _tilemap.SetOverlayTiles(positions, isGrass ? OverlayTileCategory.Grass : null);
        }

        public void SetIce(IEnumerable<Vector2Int> positions, bool isIce)
        {
            _tilemap.SetOverlayTiles(positions, isIce ? OverlayTileCategory.FloatingIce : null);
        }

        public Vector2Int FindBlankPositionFrom(Vector2Int position, Func<Vector2Int, bool> isBlankFunc)
        {
            return BlankFinder.FindBlankPosition(isBlankFunc, _tilemap.IsWalkable, position);
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

        // 視界計算（ViewCalculator）はターン進行中に敵AIの判断や表示更新から何度も呼ばれ、
        // 素朴に毎回計算すると処理が数百msに達していた。そこで原点ごとの可視マス集合をキャッシュし、
        // 同一ターン内の再計算を避ける。タイル変化時に該当範囲だけ無効化する（OnTilesChanged 参照）。
        private Dictionary<Vector2Int, HashSet<Vector2Int>> _visionCache = new();
        private HashSet<Vector2Int>? _fullVisibleArea;

        public bool IsVisible(Vector2Int from, Vector2Int to, float radius)
        {
            if ((from - to).sqrMagnitude > radius * radius)
                return false;
            // 視界は相互対称（from から to が見えるなら逆も成立）なので、
            // どちらかの可視集合がキャッシュ済みならそれを再利用し、計算回数を半減させる。
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

        // 指定地点の可視マスを計算してキャッシュに格納する。光を通さないタイル（壁など）を遮蔽として扱う。
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
        public IReadOnlyPlayerCharacter PlayerCharacter => Player.ReadOnlyCharacter;
        public IReadOnlyItem? ItemAt(Vector2Int position) => EntityManager.GetItemAt(position)?.Item;
        public IEnumerable<IReadOnlyItem> ItemsIn(IEnumerable<Vector2Int> positions) => Items.In(positions).Select(item => item.Item);
        public IObservableCollection<IEntity> Entities => EntityManager.Entities;
        public IObservableCollection<ICharacter> Characters => EntityManager.Characters;
        public IEnumerable<ILockedEntity> LockedEntities => EntityManager.LockedEntities;
        public ITerrain Terrain => _tilemap;
        public IObservableCollection<IItemEntity> Items => EntityManager.Items;
        public List<Stairs> Stairs => EntityManager.Stairs;
        public IEntity? GetEntityFastAt(Vector2Int position, EntityLayer layer) => EntityManager.GetEntityFastAt(position, layer);
        public ICharacter? GetCharacterAt(Vector2Int position) => EntityManager.GetCharacterAt(position);
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
        public IReadOnlyCollection<Vector2Int> AllCharacterPositionsFast() => EntityManager.AllCharacterPositionsFast();
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

        public void SpawnTrap(TrapData trap, Vector2Int position) => EntityManager.SpawnTrap(trap, position);
        public IItemEntity? TryPickUpAt(Vector2Int position, bool canPickUpShopItem) => EntityManager.TryPickUpAt(position, canPickUpShopItem);
        internal IEnumerable<ICharacter> GetFollowingCharacters() => EntityManager.GetFollowingCharacters();
    }
}