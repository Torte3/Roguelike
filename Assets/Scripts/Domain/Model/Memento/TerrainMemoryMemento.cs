#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.Map;
using UnityEngine;
using Utilities;
using Utilities.Serialize;

namespace Domain.Model.Memento
{
    [Serializable]
    public class TerrainMemoryMemento
    {
        [SerializeField] private string _mapId;
        [SerializeField] private SerializableDictionary<Vector2Int, TileMemento> _tiles;

        public IReadOnlyDictionary<Vector2Int, TileMemento> Tiles => _tiles;

        public TerrainMemoryMemento(Id<IMap> mapId, IDictionary<Vector2Int, TileMemento> tiles)
            : this(mapId.ToString(), tiles)
        {
        }

        private TerrainMemoryMemento(string mapId, IDictionary<Vector2Int, TileMemento> tiles)
        {
            _mapId = mapId;
            _tiles = tiles.ToSerializable();
        }

        public static TerrainMemoryMemento Empty => new("", new Dictionary<Vector2Int, TileMemento>());

        public bool IsOf(Id<IMap> mapId)
        {
            return _mapId == mapId.ToString();
        }
    }
}
