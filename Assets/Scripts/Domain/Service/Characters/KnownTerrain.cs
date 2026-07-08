#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Map;
using Domain.Model.Memento;
using UnityEngine;

namespace Domain.Service.Characters
{
    internal class KnownTerrain : ITerrain, IPassableChecker
    {
        private readonly IMap _map;
        private readonly Dictionary<Vector2Int, TileData> _outdatedTiles;

        public KnownTerrain(TerrainMemoryMemento memento, IMap map)
        {
            _map = map;
            _outdatedTiles = memento.IsOf(map.Id)
                ? memento.Tiles.ToDictionary(pair => pair.Key, pair => new TileData(pair.Value))
                : new Dictionary<Vector2Int, TileData>();
        }

        public void Remember(IEnumerable<(Vector2Int Position, TileData Tile)> previousTiles, IVisionRange vision)
        {
            foreach (var (position, tile) in previousTiles.Where(pair => !vision.IsVisible(pair.Position)))
                _outdatedTiles.TryAdd(position, tile);
        }

        public void Update(IVisionRange vision)
        {
            foreach (var position in _outdatedTiles.Keys.Where(vision.IsVisible).ToList())
                _outdatedTiles.Remove(position);
        }

        public TerrainMemoryMemento Serialize()
        {
            return new TerrainMemoryMemento(
                _map.Id,
                _outdatedTiles.ToDictionary(pair => pair.Key, pair => pair.Value.Serialize()));
        }

        public bool IsWalkable(Vector2Int position)
        {
            return _outdatedTiles.TryGetValue(position, out var tile) ? tile.IsWalkable() : _map.Terrain.IsWalkable(position);
        }

        public bool IsPassable(Vector2Int position)
        {
            return _outdatedTiles.TryGetValue(position, out var tile) ? tile.IsPassable() : _map.Terrain.IsPassable(position);
        }

        public bool IsTransparent(Vector2Int position)
        {
            return _outdatedTiles.TryGetValue(position, out var tile)
                ? tile.IsTransparent()
                : _map.Terrain.IsTransparent(position);
        }

        public IMapPosition At(Vector2Int position) => _map.At(position, this);
    }
}
