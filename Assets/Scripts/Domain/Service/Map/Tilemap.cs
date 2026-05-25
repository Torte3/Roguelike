using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Evaluation;
using Domain.Model.Map;
using Domain.Model.Memento;
using ObservableCollections;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Service.Map
{
    public class Tilemap : IDisposable, ISerializable<TilemapMemento>, ITilemap
    {
        private readonly HashSet<Vector2Int> _allWalkablePositionsSet;
        private readonly HashSet<Vector2Int> _allPassablePositionsSet;
        private readonly HashSet<Vector2Int> _allLightPassablePositionsSet;
        private readonly Subject<IEnumerable<(Vector2Int Position, TileData Tile)>> _onTilesChanged = new();

        private readonly Subject<IEnumerable<(Vector2Int Position, OverlayTileCategory? Category)>>
            _onOverlayTilesChanged = new();

        private readonly Subject<IEnumerable<(Vector2Int Position, bool IsKnown)>> _onTilesKnownChanged = new();
        private readonly ObservableDictionary<Vector2Int, TileData> _tiles;
        private readonly ObservableDictionary<Vector2Int, OverlayTileCategory> _overlayTiles;
        private TilemapMemento _mementoCache;
        public readonly int Height;
        public readonly int Width;

        public void UpdateChunk(Vector2Int position)
        {
            return;
        }

        public Tilemap(TilemapMemento memento)
        {
            _tiles = memento.Tiles;
            Width = _tiles.Max(pair => pair.Key.x) - _tiles.Min(pair => pair.Key.x) + 1;
            Height = _tiles.Max(pair => pair.Key.y) - _tiles.Min(pair => pair.Key.y) + 1;
            _overlayTiles = memento.OverlayTiles;

            _allWalkablePositionsSet = FindAllWalkablePositions().ToHashSet();
            _allPassablePositionsSet = FindAllPassablePositions().ToHashSet();
            _allLightPassablePositionsSet = FindAllLightPassablePositions().ToHashSet();

            OnTilesChanged.Subscribe(changeTiles =>
            {
                foreach (var (position, tileData) in changeTiles)
                {
                    if (tileData.IsWalkable())
                        _allWalkablePositionsSet.Add(position);
                    else
                        _allWalkablePositionsSet.Remove(position);

                    if (tileData.IsPassable())
                        _allPassablePositionsSet.Add(position);
                    else
                        _allPassablePositionsSet.Remove(position);

                    if (tileData.IsTransparent())
                        _allLightPassablePositionsSet.Add(position);
                    else
                        _allLightPassablePositionsSet.Remove(position);

                    ResetMask(position);
                }

                UpdateMementoCache();
            });
            OnOverlayTilesChanged.Subscribe(changeOverlayTiles => { UpdateMementoCache(); });
            OnTilesKnownChanged.Subscribe(changeTiles => { UpdateMementoCache(); });
            UpdateMementoCache();
        }

        public Tilemap(int width, int height)
        {
            Width = width;
            Height = height;
            _tiles = new(new RectInt(Vector2Int.zero, new Vector2Int(width, height)).RectRange()
                .ToDictionary(x => x, _ => new TileData(TileData.Build(MapType.Cave, TileCategory.Blank, false))));
        }

        public Vector2Int Size => new(Width, Height);

        public void Dispose()
        {
            _onTilesChanged.Dispose();
            _onTilesKnownChanged.Dispose();
        }

        private void UpdateMementoCache()
        {
            _mementoCache = new TilemapMemento
            (
                "",
                _tiles,
                _overlayTiles
            );
        }

        public void UpdateTurn()
        {
            var grasses = new List<Vector2Int>();
            foreach (var (position, _) in _overlayTiles.Where(pair => pair.Value == OverlayTileCategory.Grass))
            {
                if (RandUtils.IsLessThanProbability(CommonSenseParameters.SpawnGrassProbabilityPerTurn))
                {
                    var spawnPosition = position + DirectionMethods.AllDirections.GetAtRandom().Vector();
                    grasses.Add(spawnPosition);
                }
            }

            SetOverlayTiles(grasses, OverlayTileCategory.Grass);
        }

        public TilemapMemento Serialize()
        {
            return _mementoCache;
        }

        public Observable<IEnumerable<(Vector2Int Position, TileData Tile)>> OnTilesChanged => _onTilesChanged;
        public Observable<IEnumerable<(Vector2Int Position, TileData Tile)>> OnTilesLoaded => Observable.Never<IEnumerable<(Vector2Int Position, TileData Tile)>>();

        public Observable<IEnumerable<(Vector2Int Position, OverlayTileCategory? Category)>> OnOverlayTilesChanged =>
            _onOverlayTilesChanged;

        public Observable<IEnumerable<(Vector2Int Position, bool IsKnown)>> OnTilesKnownChanged => _onTilesKnownChanged;
        public ReadOnlyReactiveProperty<RectInt> Rect => new ReactiveProperty<RectInt>(new RectInt(Vector2Int.zero, Size));

        public IEnumerable<(Vector2Int position, TileData tileData)> GetAllTiles()
        {
            return _tiles.Select(pair => (pair.Key, pair.Value));
        }

        public IEnumerable<Vector2Int> GetAllGrasses()
        {
            return _overlayTiles.Where(pair => pair.Value == OverlayTileCategory.Grass).Select(pair => pair.Key);
        }

        public IEnumerable<Vector2Int> GetAllIces()
        {
            return _overlayTiles.Where(pair => pair.Value == OverlayTileCategory.FloatingIce).Select(pair => pair.Key);
        }

        public bool IsGrass(Vector2Int position)
        {
            return _overlayTiles.ContainsKey(position) && _overlayTiles[position] == OverlayTileCategory.Grass;
        }

        public bool IsIce(Vector2Int position)
        {
            return _overlayTiles.ContainsKey(position) && _overlayTiles[position] == OverlayTileCategory.FloatingIce;
        }

        public bool IsWalkable(Vector2Int position)
        {
            if (GetTile(position).MapOr(false, tile => tile.IsWalkable()))
                return true;
            if (_overlayTiles.ContainsKey(position) && _overlayTiles[position] == OverlayTileCategory.FloatingIce)
                return true;
            return false;
        }

        public bool IsPassable(Vector2Int position)
        {
            return GetTile(position).MapOr(false, tile => tile.IsPassable());
        }

        public bool IsTransparent(Vector2Int position)
        {
            return GetTile(position).MapOr(false, tile => tile.IsTransparent());
        }

        public HashSet<Vector2Int> GetAllWalkablePositions()
        {
            return new HashSet<Vector2Int>(_allWalkablePositionsSet);
        }

        public HashSet<Vector2Int> GetAllPassablePositions()
        {
            return new HashSet<Vector2Int>(_allPassablePositionsSet);
        }

        public HashSet<Vector2Int> GetAllLightPassablePositions()
        {
            return new HashSet<Vector2Int>(_allLightPassablePositionsSet);
        }

        public bool IsPositionInsideMap(Vector2Int position)
        {
            return _tiles.ContainsKey(position);
        }

        public bool IsPositionInsideActiveChunk(Vector2Int position)
        {
            return IsPositionInsideMap(position);
        }

        public Option<TileData> GetTile(Vector2Int position)
        {
            if (!IsPositionInsideMap(position))
            {
                Log.Info($"position {position} is out of map (MapSize Width:{Width}, Height:{Height})");
                return Option<TileData>.None;
            }

            return Option.Some(_tiles[position]);
        }

        private IEnumerable<Vector2Int> FindAllWalkablePositions()
        {
            return GetAllTiles().Where(pair => pair.tileData.IsWalkable()).Select(pair => pair.position);
        }

        private IEnumerable<Vector2Int> FindAllPassablePositions()
        {
            return GetAllTiles().Where(pair => pair.tileData.IsPassable()).Select(pair => pair.position);
        }

        private IEnumerable<Vector2Int> FindAllLightPassablePositions()
        {
            return GetAllTiles().Where(pair => pair.tileData.IsTransparent()).Select(pair => pair.position);
        }

        public void SetOverlayTiles(IEnumerable<Vector2Int> positions, OverlayTileCategory? category)
        {
            var result = new List<(Vector2Int position, OverlayTileCategory? category)>();
            foreach (var position in positions)
            {
                OverlayTileCategory? currentCategory =
                    _overlayTiles.ContainsKey(position) ? _overlayTiles[position] : null;
                if (category != currentCategory)
                {
                    if (category != null)
                    {
                        if (GetTile(position).MapOr(false,
                                tile => tile.Category() == category.Value.GetPlaceableTileCategory()))
                        {
                            _overlayTiles[position] = category.Value;
                            result.Add((position, category.Value));
                        }
                    }
                    else
                    {
                        _overlayTiles.Remove(position);
                        result.Add((position, null));
                    }
                }
            }

            _onOverlayTilesChanged.OnNext(result);
        }

        public void SetTilesKnown(IEnumerable<Vector2Int> positions, bool isKnown)
        {
            var changedPositions = positions
                .Select(position => (position, GetTile(position)))
                .Where(pair => pair.Item2.MapOr(false, tile => tile.IsKnown != isKnown))
                .Select(pair => (pair.position, pair.Item2.Expect("tile is null")));
            var result = new List<(Vector2Int position, bool isKnown)>();
            foreach (var (position, tile) in changedPositions)
            {
                tile.SetKnown(isKnown);
                result.Add((position, isKnown));
            }

            _onTilesKnownChanged.OnNext(result);
        }

        public void RemoveWalls(IEnumerable<Vector2Int> positions)
        {
            var changedPositions = positions
                .Select(position => (position, GetTile(position)))
                .Where(pair => pair.Item2.MapOr(false, tile => tile.Category() == TileCategory.Wall))
                .Select(pair => (pair.position, pair.Item2.Expect("tile is null")));
            var result = new List<(Vector2Int position, TileData tileData)>();
            foreach (var (position, tile) in changedPositions)
            {
                _tiles[position] = new TileData(TileData.Build(tile.MapType, TileCategory.Floor, false));
                result.Add((position, _tiles[position]));
            }

            _onTilesChanged.OnNext(result);
        }

        public void ResetMask(Vector2Int position)
        {
            SetTilesKnown(new RectInt(position - new Vector2Int(1, 1), new Vector2Int(3, 3)).RectRange(), false);
        }
    }
}