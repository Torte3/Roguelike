#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Dungeon;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Characters;
using Domain.Service.Events;
using Domain.Service.Items;
using Domain.Service.Map;
using Domain.Service.Rooms;
using RandomDungeonWithBluePrint;
using Unity.Logging;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using Utilities.Serialize.Option;
using Random = UnityEngine.Random;

namespace Game
{
    public class MapBuilder
    {
        private readonly ItemMarketPriceTable _marketPriceTable;
        private readonly Id<IMap> _mapId;
        private readonly TilemapBuilder _tilemap;
        private readonly List<CharacterMemento> _characters = new();
        private readonly List<MimicItemMemento> _mimicItems = new();
        private readonly List<MimicMoneyMemento> _mimicMoney = new();
        private readonly List<MimicStairsMemento> _mimicStairs = new();
        private readonly List<ItemEntityMemento> _items = new();
        private readonly List<StairsMemento> _stairs = new();
        private readonly List<ChestMemento> _chests = new();
        private readonly List<TrapMemento> _traps = new();
        private readonly List<StatueMemento> _statues = new();
        private readonly List<MoneyMemento> _money = new();
        private BonfireMemento? _bonfire;
        private MagicPotMemento? _magicPot;
        private WorkbenchMemento? _workbench;
        private EntityMemento? _teleporter;
        private readonly List<Id<IEntity>> _keyCharacters = new();
        private readonly RoomMemento? _monsterHouse;
        private readonly ShopMemento? _shop;
        private readonly List<Id<Room>> _canPlaceStairRooms = new();
        private readonly Dictionary<Id<Room>, HashSet<Vector2Int>> _blankPositionsInRooms;

        private readonly float _progress;

        public MapBuilder(FieldBluePrint bluePrint, float waterChance, FloorSpec data, float progress, Id<IMap> mapId)
        {
            _progress = progress;
            _marketPriceTable = Addressables.LoadAssetAsync<ItemMarketPriceTable>("Assets/Database/ItemData/ItemMarketPriceTable.asset")
                .WaitForCompletion();
            _tilemap = new TilemapBuilder(data.Type, bluePrint, waterChance);
            _mapId = mapId;
            _blankPositionsInRooms = new Dictionary<Id<Room>, HashSet<Vector2Int>>();

            var roomIds = _tilemap.RoomIds.ToList();
            var grassRoomIds = _tilemap.RoomIds.ToList();
            var stairEligibleSpecialRooms = new List<Id<Room>>();

            foreach (var isolateRoomId in _tilemap.IsolateRooms)
            {
                if (roomIds.Count() > 1)
                {
                    CreateIsolateRoom(data, isolateRoomId);
                    roomIds.Remove(isolateRoomId);
                }
            }

            if (RandUtils.IsLessThanProbability(data.ShopChance) && roomIds.Count() > 1)
            {
                var shopRoom = roomIds.GetAtRandom();
                _shop = CreateShop(data, shopRoom);
                if (_shop != null)
                {
                    roomIds.Remove(shopRoom);
                    grassRoomIds.Remove(shopRoom);
                }
            }

            if (RandUtils.IsLessThanProbability(data.MonsterHouseChance) && roomIds.Count() > 1)
            {
                var monsterHouseRoom = roomIds.GetAtRandom();
                _monsterHouse = CreateMonsterHouse(data, monsterHouseRoom);
                if (_monsterHouse != null)
                {
                    stairEligibleSpecialRooms.Add(monsterHouseRoom);
                    roomIds.Remove(monsterHouseRoom);
                }
            }

            if (RandUtils.IsLessThanProbability(data.RestRoomChance) && roomIds.Count() > 1)
            {
                var restRoom = roomIds.GetAtRandom();
                if (CreateRestRoom(data, restRoom))
                {
                    stairEligibleSpecialRooms.Add(restRoom);
                    roomIds.Remove(restRoom);
                }
            }

            if (RandUtils.IsLessThanProbability(data.LakeChance) && roomIds.Count() > 1)
            {
                var lakeRoom = roomIds.GetAtRandom();
                if (CreateLakeRoom(data, lakeRoom))
                {
                    stairEligibleSpecialRooms.Add(lakeRoom);
                    roomIds.Remove(lakeRoom);
                }
            }

            foreach (var room in roomIds)
            {
                CreateRoom(data, room);
            }

            _canPlaceStairRooms = roomIds.Concat(stairEligibleSpecialRooms).ToList();

            if (RandUtils.IsLessThanProbability(data.ShinyChance))
            {
                AddShinyToRoom(data, roomIds.GetAtRandom());
            }

            if (data.ExistBoss)
            {
                var bossRoom = roomIds.GetAtRandom();
                foreach (var bossData in data.Boss)
                {
                    var boss = CharacterFactory.BuildCharacter(
                        bossData,
                        GetRandomBlankPositionInRoom(bossRoom),
                        isSlept: false,
                        isShiny: false);
                    _characters.Add(boss);
                    _keyCharacters.Add(new Id<IEntity>(boss.Entity.Id));
                }
                
                if (data.BossReward.Any())
                {
                    var chestPosition = GetRandomBlankPositionInRoom(bossRoom);
                    var bossItems = data.BossReward.Select(itemData => itemData.Build()).ToList();
                    
                    _chests.Add(Chest.Build(bossItems, chestPosition, _keyCharacters));
                }
            }

            foreach (var room in grassRoomIds)
                AddGrasses(data, room);
        }

        private HashSet<Vector2Int> GetAllBlankPositionInRoom(Id<Room> roomId)
        {
            if (!_blankPositionsInRooms.ContainsKey(roomId))
                _blankPositionsInRooms[roomId] = _tilemap.GetWalkablePositionsIn(roomId);
            return _blankPositionsInRooms[roomId];
        }

        private Vector2Int GetRandomBlankPositionInRoom(Id<Room> roomId)
        {
            if (!_blankPositionsInRooms.ContainsKey(roomId))
                _blankPositionsInRooms[roomId] = _tilemap.GetWalkablePositionsIn(roomId);
            var position = _blankPositionsInRooms[roomId].GetAtRandom();
            _blankPositionsInRooms[roomId].Remove(position);
            return position;
        }

        private IEnumerable<Vector2Int> GetRandomBlankPositionsInRoom(Id<Room> roomId, int count)
        {
            if (!_blankPositionsInRooms.ContainsKey(roomId))
                _blankPositionsInRooms[roomId] = _tilemap.GetWalkablePositionsIn(roomId);
            var positions = _blankPositionsInRooms[roomId].GetAtRandom(count);
            foreach (var position in positions)
                _blankPositionsInRooms[roomId].Remove(position);
            return positions;
        }

        public Vector2Int GetRandomStairPosition()
        {
            return GetRandomBlankPositionInRoom(_canPlaceStairRooms.GetAtRandom());
        }

        private void AddGrasses(FloorSpec data, Id<Room> roomId)
        {
            var randomValue = Random.value * 1024;
            foreach (var position in _tilemap.GetWalkablePositionsIn(roomId))
            {
                if (data.GrassChance == 1 ||
                    Mathf.Clamp01(Mathf.PerlinNoise(position.x / 8f + randomValue, position.y / 8f + randomValue)) <
                    data.GrassChance)
                {
                    _tilemap.SetGrasses(new[] { position }, true);
                }
            }
        }

        private void AddIce(Vector2Int position)
        {
            _tilemap.SetIces(new[] { position }, true);
        }

        private void CreateRoom(FloorSpec data, Id<Room> roomId)
        {
            if (data.RoundRoomCorner)
                _tilemap.RoundRoomCorner(roomId);
            else if (data.CaveInOneRoom)
                _tilemap.CaveInOneRoom(roomId);

            var itemCount = data.ItemCount();
            var moneyCount = data.MoneyCount();
            var characterCount = data.CharacterCount();
            var trapCount = data.TrapCount();

            AddItemsToRoom(data, roomId, itemCount);
            AddMoneyToRoom(data, roomId, moneyCount);
            AddCharactersToRoom(data, roomId, characterCount);
            AddTrapsToRoom(data, roomId, trapCount);
            if (RandUtils.IsLessThanProbability(data.ChestChance))
                AddChestToRoom(data, roomId);
            if (RandUtils.IsLessThanProbability(data.StatueChance))
                AddStatueToRoom(data, roomId);
        }

        private bool CreateLakeRoom(FloorSpec data, Id<Room> roomId)
        {
            if (data.RoundRoomCorner)
                _tilemap.RoundRoomCorner(roomId);

            var lakeSize = new Vector2Int(Random.Range(4, 6), Random.Range(4, 6));
            var innerRect = _tilemap.GetWalkablePositionsIn(roomId).GetRandomInnerRect(lakeSize + Vector2Int.one * 2);
            if (innerRect == null)
            {
                return false;
            }

            var lakeRect = new RectInt(innerRect.Value.min + Vector2Int.one, lakeSize);
            var islandRect = new RectInt(lakeRect.min + Vector2Int.one, lakeSize - Vector2Int.one * 2);
            _tilemap.SetWater(lakeRect.RectRange().Where(position => !islandRect.Contains(position)));
            GetAllBlankPositionInRoom(roomId).RemoveWhere(position => lakeRect.Contains(position));

            var teleporterPosition = islandRect.RectRange().GetAtRandom();
            _teleporter = Teleporter.Build(teleporterPosition);
            var itemPositions = islandRect.RectRange().Where(position => position != teleporterPosition);
            foreach (var position in itemPositions)
            {
                var item = data.ItemDatabase.GetRandomItem(_progress);
                _items.Add(ItemEntity.Build(position, BuildFloorItem(data, item)));
            }

            var itemCount = data.ItemCount();
            var moneyCount = data.MoneyCount();
            var characterCount = data.CharacterCount();
            var trapCount = data.TrapCount();

            AddItemsToRoom(data, roomId, itemCount);
            AddMoneyToRoom(data, roomId, moneyCount);
            AddCharactersToRoom(data, roomId, characterCount);
            AddTrapsToRoom(data, roomId, trapCount);
            if (RandUtils.IsLessThanProbability(data.ChestChance))
                AddChestToRoom(data, roomId);
            if (RandUtils.IsLessThanProbability(data.StatueChance))
                AddStatueToRoom(data, roomId);

            return true;
        }

        private ShopMemento? CreateShop(FloorSpec data, Id<Room> roomId)
        {
            var shopItems = data.ShopItems.GetRandomItem().Items;

            var width = Random.Range(2, 5);
            var height = Random.Range(2, 5);
            var rect = _tilemap.GetCenteredInnerRect(roomId, new Vector2Int(width, height));

            if (rect == null)
            {
                return null;
            }

            foreach (var position in rect.Value.RectRange())
            {
                var item = shopItems.GetRandomItem(_progress);
                _items.Add(ItemEntity.Build(position, Item.Build(item, state: ItemState.ShopItem)));
                GetAllBlankPositionInRoom(roomId).Remove(position);
            }

            var clerkPosition = GetRandomBlankPositionInRoom(roomId);
            var clerk = CharacterFactory.BuildCharacter(data.Clerk, clerkPosition, isSlept: false, isShiny: false,
                homeLocation: new Location(_mapId, clerkPosition));
            _characters.Add(clerk);

            return Shop.Build(_tilemap.GetRoom(roomId), new Id<IEntity>(clerk.Entity.Id), _items.ToList(), _marketPriceTable);
        }

        private RoomMemento? CreateMonsterHouse(FloorSpec data, Id<Room> roomId)
        {
            if (data.RoundRoomCorner)
                _tilemap.RoundRoomCorner(roomId);

            AddItemsToRoom(data, roomId, 5);
            AddMoneyToRoom(data, roomId, 3);
            AddTrapsToRoom(data, roomId, 3);

            AddChestToRoom(data, roomId);

            return MonsterHouse.Build(_tilemap.GetRoom(roomId));
        }

        private bool CreateRestRoom(FloorSpec data, Id<Room> roomId)
        {
            if (data.RoundRoomCorner)
                _tilemap.RoundRoomCorner(roomId);

            var innerRect = _tilemap.GetWalkablePositionsIn(roomId).GetRandomInnerRect(new Vector2Int(5, 5));
            if (innerRect == null)
            {
                return false;
            }

            var center = innerRect.Value.min + VectorExtension.FloorToInt(innerRect.Value.size / 2);

            switch (RandUtils.WeightedIndex(data.MagicPotWeight, data.WorkbenchWeight, data.BonfireWeight))
            {
                case 0:
                    _magicPot = MagicPot.Build(center);
                    break;
                case 1:
                    _workbench = Workbench.Build(center);
                    break;
                case 2:
                    _bonfire = Bonfire.Build(center);
                    break;
                default:
                    Log.Warning("Invalid weight");
                    _bonfire = Bonfire.Build(center);
                    break;
            }

            foreach (var position in innerRect.Value.RectRange())
            {
                GetAllBlankPositionInRoom(roomId).Remove(position);
            }

            foreach (var direction in DirectionMethods.AllDirections.GetAtRandom(Random.Range(1, 3)))
            {
                var position = center + direction.Vector();
                var character = CharacterFactory.BuildCharacter(
                    data.Npcs.GetRandomItem(),
                    position,
                    direction: direction.Reverse(),
                    isSlept: RandUtils.IsLessThanProbability(data.SleepChance),
                    isShiny: false,
                    homeLocation: new Location(_mapId, center));
                _characters.Add(character);
            }

            var itemCount = data.ItemCount();
            var moneyCount = data.MoneyCount();
            var trapCount = data.TrapCount();

            AddItemsToRoom(data, roomId, itemCount);
            AddMoneyToRoom(data, roomId, moneyCount);
            AddTrapsToRoom(data, roomId, trapCount);

            return true;
        }

        private void AddCharactersToRoom(FloorSpec data, Id<Room> roomId, int count)
        {
            foreach (var position in GetRandomBlankPositionsInRoom(roomId, count))
            {
                if (data.Enemies.Count == 0)
                    break;
                var enemy = data.Enemies.GetRandomItem();
                if (enemy.CanMimic)
                {
                    switch (enemy.MimicWeights.GetRandomIndex())
                    {
                        case 0:
                            var item = data.ItemDatabase.GetRandomItem(_progress);
                            _mimicItems.Add(MimicItemEntity.Build(ItemEntity.Build(position, item.Build()), enemy));
                            break;
                        case 1:
                            item = data.ItemDatabase.GetRandomItem(_progress);
                            _items.Add(ItemEntity.Build(position, item.Build(mimic: enemy)));
                            break;
                        case 2:
                            _mimicMoney.Add(MimicMoney.Build(position, data.MoneyAmount(), enemy));
                            break;
                        case 3:
                            _mimicStairs.Add(MimicStairs.Build(MovementEntityType.DownStairs, position, enemy));
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                else
                {
                    var character = CharacterFactory.BuildCharacter(
                        data.Enemies.GetRandomItem(),
                        position,
                        isSlept: RandUtils.IsLessThanProbability(data.SleepChance),
                        isShiny: false);
                    _characters.Add(character);
                }
            }
        }

        private void AddItemsToRoom(FloorSpec data, Id<Room> roomId, int count)
        {
            foreach (var position in GetRandomBlankPositionsInRoom(roomId, count))
            {
                var item = data.ItemDatabase.GetRandomItem(_progress);
                _items.Add(ItemEntity.Build(position, BuildFloorItem(data, item)));
            }
        }

        private static IItemMemento BuildFloorItem(FloorSpec data, IItemData itemData)
        {
            var isCursed = RandUtils.IsLessThanProbability(data.CursedItemChance);
            return itemData.Build(isCursed: isCursed);
        }

        public void AddMoneyToRoom(FloorSpec data, Id<Room> roomId, int count)
        {
            foreach (var position in GetRandomBlankPositionsInRoom(roomId, count))
            {
                _money.Add(Money.Build(position, data.MoneyAmount()));
            }
        }

        private void AddTrapsToRoom(FloorSpec data, Id<Room> roomId, int count)
        {
            foreach (var position in GetRandomBlankPositionsInRoom(roomId, count))
            {
                _traps.Add(Trap.Build(data.Traps.GetRandomItem(), position));
            }
        }

        private void AddShinyToRoom(FloorSpec data, Id<Room> roomId)
        {
            var position = GetRandomBlankPositionInRoom(roomId);
            var weaponData = data.ItemDatabase.GetRandomItem(ItemCategory.Weapons, _progress);
            var item = weaponData.Build(
                upgradeCount: Random.Range(1, 5),
                isCursed: false);
            var character = CharacterFactory.BuildCharacter(
                data.Enemies.GetRandomItem(),
                position,
                item,
                isShiny: true
            );
            _characters.Add(character);
        }

        private void AddChestToRoom(FloorSpec data, Id<Room> roomId)
        {
            var position = GetRandomBlankPositionInRoom(roomId);
            if (RandUtils.IsLessThanProbability(data.MimicChance))
            {
                _chests.Add(Chest.Build(data.Mimic, position));
                return;
            }

            if (RandUtils.IsLessThanProbability(data.WeaponChanceInChest))
            {
                var weaponData = data.ItemDatabase.GetRandomItem(ItemCategory.Weapons, _progress);
                if (weaponData is DirectWeaponData directWeapon)
                {
                    var weapon = DirectWeapon.Build(
                        directWeapon,
                        prefix: data.WeaponPrefixes.GetRandomItem(_progress),
                        isCursed: false);
                    _chests.Add(Chest.Build(weapon, position));
                }
                else if (weaponData is RangedWeaponData rangedWeapon)
                {
                    var weapon = RangedWeapon.Build(
                        rangedWeapon,
                        prefix: data.WeaponPrefixes.GetRandomItem(_progress),
                        isCursed: false);
                    _chests.Add(Chest.Build(weapon, position));
                }
                else
                {
                    _chests.Add(Chest.Build(weaponData, position));
                }
            }
            else
            {
                _chests.Add(Chest.Build(data.ChestItems.GetRandomItem(_progress), position));
            }
        }

        private void AddStatueToRoom(FloorSpec data, Id<Room> roomId)
        {
            var position = GetRandomBlankPositionInRoom(roomId);
            _statues.Add(Statue.Build(data.Statues.GetRandomItem(), position));
        }

        private bool CreateIsolateRoom(FloorSpec data, Id<Room> roomId)
        {
            var teleporterPosition = GetRandomBlankPositionInRoom(roomId);
            _teleporter = Teleporter.Build(teleporterPosition);

            var walkablePositions = _tilemap.GetWalkablePositionsIn(roomId);
            var moneyPositions = walkablePositions.Where(p => p != teleporterPosition).ToList();
            foreach (var position in GetRandomBlankPositionsInRoom(roomId, moneyPositions.Count))
            {
                _money.Add(Money.Build(position, data.MoneyAmount()));
            }

            return true;
        }

        public void AddMovementEntity(MovementData data)
        {
            if (data.Id != null && data.DestinationId != null)
                _stairs.Add(Stairs.Build(data.Type, GetRandomStairPosition(), data.Id,
                    data.Destination, data.DestinationId));
            else
                _stairs.Add(Stairs.Build(data.Type, GetRandomStairPosition(), data.Destination));
        }

        public MapMemento Build()
        {
            return new MapMemento
            (
                _mapId,
                _tilemap.Build(),
                new EntitiesMemento(
                    _characters,
                    _items,
                    EventEntityManager.Build(
                        _mimicItems,
                        _mimicMoney,
                        _mimicStairs,
                        _stairs,
                        _chests,
                        _traps,
                        _statues,
                        _money,
                        _bonfire.ToOption(),
                        _magicPot.ToOption(),
                        _workbench.ToOption(),
                        _teleporter.ToOption()),
                    FireEntityManager.Build()),
                _monsterHouse.ToOption(),
                _shop.ToOption(),
                _blankPositionsInRooms.Values.SelectMany(positions => positions).GetAtRandom()
            );
        }
    }
}