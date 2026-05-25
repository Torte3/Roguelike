#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Map;
using Domain.Model.Memento;
using RandomDungeonWithBluePrint;
using UnityEngine;
using Utilities;
using static RandomDungeonWithBluePrint.Constants;

namespace Domain.Service.Map
{
    public class TilemapBuilder
    {
        private readonly int _width;
        private readonly MapType _mapType;
        private readonly TileCategory[] _tiles;
        private readonly Dictionary<Vector2Int, OverlayTileCategory> _overlayTiles = new();
        private readonly float _waterChance;
        private readonly float _randomValueForWater;
        private readonly Dictionary<Id<Room>, RectInt> _rooms = new();
        private readonly Dictionary<int, Id<Room>> _sectionToRoomMap = new();
        private readonly Field _field;
        private readonly List<Id<Room>> _isolateRooms = new();
        public List<Id<Room>> RoomIds => _rooms.Keys.ToList();
        public List<Id<Room>> IsolateRooms => _isolateRooms;

        public TilemapBuilder(MapType mapType, FieldBluePrint bluePrint, float waterChance)
        {
            _field = FieldBuilder.Build(bluePrint);
            _width = _field.Grid.Size.x + 2;
            _mapType = mapType;
            _tiles = new TileCategory[_width * (_field.Grid.Size.y + 2)];
            _waterChance = waterChance;
            _randomValueForWater = Random.value * 1024;
            for (var x = -1; x < _field.Grid.Size.x + 1; x++)
            {
                for (var y = -1; y < _field.Grid.Size.y + 1; y++)
                {
                    TileCategory tileType;
                    if (x == -1 || y == -1 || x == _field.Grid.Size.x || y == _field.Grid.Size.y)
                    {
                        tileType = TileCategory.UnbreakableWall;
                    }
                    else
                    {
                        var mapChipType = _field.Grid[x, y];
                        tileType = mapChipType == (int)MapChipType.Wall
                            ? GetNotWalkableCategory(x, y)
                            : TileCategory.Floor;
                    }

                    _tiles[x + 1 + (y + 1) * _width] = tileType;
                }
            }

            foreach (var section in _field.Sections.Where(s => s.ExistRoom))
            {
                var roomRect = new RectInt(section.Room.Rect.position + new Vector2Int(1, 1), section.Room.Rect.size);
                var roomId = Id<Room>.Generate();
                _rooms[roomId] = roomRect;
                _sectionToRoomMap[section.Index] = roomId;
            }

            // 孤立したSectionを検出してIsolateRoomとして処理
            ProcessIsolatedSections();
        }

        private TileCategory GetNotWalkableCategory(int x, int y)
        {
            if (_waterChance == 1 ||
                Mathf.Clamp01(Mathf.PerlinNoise(x / 16f + _randomValueForWater, y / 16f + _randomValueForWater)) <
                _waterChance)
            {
                return TileCategory.Water;
            }

            return TileCategory.Wall;
        }

        public void CaveInOneRoom(Id<Room> roomId)
        {
            var room = _rooms[roomId];
            var randomValue = Random.value * 1024;
            foreach (var position in room.RectRange())
            {
                var distanceFromEdgeX = Mathf.Min(position.x - room.xMin, room.xMax - position.x);
                var distanceFromEdgeY = Mathf.Min(position.y - room.yMin, room.yMax - position.y);
                var distanceFromEdge = Mathf.Min(distanceFromEdgeX, distanceFromEdgeY);

                var value = Mathf.Clamp01(Mathf.PerlinNoise(position.x / 8f + randomValue,
                    position.y / 8f + randomValue));
                value += Mathf.Pow(0.5f, distanceFromEdge / 4f);
                if (value > 0.5f)
                {
                    _tiles[position.x + position.y * _width] = TileCategory.Wall;
                }
            }
        }

        public void RoundRoomCorner(Id<Room> roomId)
        {
            var room = _rooms[roomId];
            RoundCorner(new Vector2Int(room.xMin, room.yMin), Direction8.DownLeft, room.size);
            RoundCorner(new Vector2Int(room.xMax - 1, room.yMin), Direction8.DownRight, room.size);
            RoundCorner(new Vector2Int(room.xMin, room.yMax - 1), Direction8.UpLeft, room.size);
            RoundCorner(new Vector2Int(room.xMax - 1, room.yMax - 1), Direction8.UpRight, room.size);
        }

        private void RoundCorner(Vector2Int position, Direction8 corner, Vector2Int roomsize)
        {
            var directionX = new Vector2Int(-corner.Vector().x, 0);
            var directionY = new Vector2Int(0, -corner.Vector().y);
            ProcessDirection(position, corner, roomsize.x / 2, directionX, directionY);
            ProcessDirection(position, corner, roomsize.y / 2, directionY, directionX);
        }

        private void ProcessDirection(Vector2Int position, Direction8 corner, int maxDimension,
            Vector2Int primaryDirection, Vector2Int secondaryDirection)
        {
            var secondaryIncrement = 0;
            while (maxDimension >= 1)
            {
                var randDimension = Random.Range(1, maxDimension);
                for (var primaryIncrement = 0; primaryIncrement < randDimension; primaryIncrement++)
                {
                    var newPosition = position + primaryIncrement * primaryDirection +
                                      secondaryIncrement * secondaryDirection;
                    if (IsSafeToPlaceWall(newPosition, corner))
                        _tiles[newPosition.x + newPosition.y * _width] =
                            GetNotWalkableCategory(newPosition.x, newPosition.y);
                    else
                        break;
                }

                maxDimension /= 2;
                secondaryIncrement += 1;
            }
        }

        private bool IsSafeToPlaceWall(Vector2Int position, Direction8 corner)
        {
            if (position.x + corner.Vector().x < 0
                || position.y + corner.Vector().y < 0
                || position.x + corner.Vector().x >= _width
                || position.y + corner.Vector().y >= _tiles.Length / _width)
            {
                return false;
            }

            if (_tiles[position.x + corner.Vector().x + position.y * _width] == TileCategory.Floor
                || _tiles[position.x + (position.y + corner.Vector().y) * _width] == TileCategory.Floor)
            {
                return false;
            }

            return true;
        }

        public HashSet<Vector2Int> GetWalkablePositionsIn(Id<Room> roomId)
        {
            var room = _rooms[roomId];
            return room.RectRange().Where(position => _tiles[position.x + position.y * _width] == TileCategory.Floor)
                .ToHashSet();
        }

        public RectInt GetRoom(Id<Room> roomId)
        {
            return _rooms[roomId];
        }

        public RectInt? GetCenteredInnerRect(Id<Room> roomId, Vector2Int size)
        {
            var rect = _rooms[roomId].GetCenteredInnerRect(size);
            if (rect.RectRange().Any(position => _tiles[position.x + position.y * _width] != TileCategory.Floor))
            {
                return null;
            }

            return rect;
        }

        public void SetGrasses(IEnumerable<Vector2Int> positions, bool isGrass)
        {
            foreach (var position in positions)
            {
                var isAlreadyGrass = _overlayTiles.ContainsKey(position) &&
                                     _overlayTiles[position] == OverlayTileCategory.Grass;
                if (isGrass != isAlreadyGrass)
                {
                    if (isGrass)
                    {
                        if (_tiles[position.x + position.y * _width] == TileCategory.Floor)
                        {
                            _overlayTiles[position] = OverlayTileCategory.Grass;
                        }
                    }
                    else
                    {
                        _overlayTiles.Remove(position);
                    }
                }
            }
        }

        public void SetIces(IEnumerable<Vector2Int> positions, bool isIce)
        {
            foreach (var position in positions)
            {
                var isAlreadyIce = _overlayTiles.ContainsKey(position) &&
                                   _overlayTiles[position] == OverlayTileCategory.FloatingIce;
                if (isIce != isAlreadyIce)
                {
                    if (isIce)
                    {
                        if (_tiles[position.x + position.y * _width] == TileCategory.Water)
                        {
                            _overlayTiles[position] = OverlayTileCategory.FloatingIce;
                        }
                    }
                    else
                    {
                        _overlayTiles.Remove(position);
                    }
                }
            }
        }

        public void SetWater(IEnumerable<Vector2Int> positions)
        {
            foreach (var position in positions)
            {
                _tiles[position.x + position.y * _width] = TileCategory.Water;
            }
        }

        public HashSet<Vector2Int> GetAllWalkablePositions()
        {
            return _tiles.Select((tile, index) => (tile, index))
                .Where(tile => tile.tile == TileCategory.Floor)
                .Select(tile => new Vector2Int(tile.index % _width, tile.index / _width))
                .ToHashSet();
        }

        public TilemapMemento Build()
        {
            return new TilemapMemento(
                _width,
                _tiles.Select(tile => TileData.Build(_mapType, tile, false)).ToArray(),
                _overlayTiles
            );
        }

        private void ProcessIsolatedSections()
        {
            var isolatedSections = _field.Sections.Where(section => _field.IsIsolatedSection(section)).ToList();

            foreach (var isolatedSection in isolatedSections)
            {
                // 孤立したSectionに対応するRoomIdを取得
                var roomId = GetRoomIdForSection(isolatedSection);
                if (roomId != null)
                {
                    _isolateRooms.Add(roomId);
                }
            }
        }

        private Id<Room>? GetRoomIdForSection(Section section)
        {
            // SectionのIndexからRoomIdを取得
            return _sectionToRoomMap.TryGetValue(section.Index, out var roomId) ? roomId : null;
        }

        public static TilemapMemento Build(string seed)
        {
            return new TilemapMemento(
                seed,
                new Dictionary<Vector2Int, TileData>(),
                new Dictionary<Vector2Int, OverlayTileCategory>()
            );
        }
    }
}