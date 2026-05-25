#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Map;
using ObservableCollections;
using UnityEngine;
using Utilities.Serialize;

namespace Domain.Model.Memento
{
    [Serializable]
    public class TilemapMemento
    {
        [field: SerializeField] public string Seed { get; private set; }
        [SerializeField] private SerializableDictionary<Vector2Int, TileMemento> _tiles;
        public ObservableDictionary<Vector2Int, TileData> Tiles =>
            new(_tiles.ToDictionary(pair => pair.Key, pair => new TileData(pair.Value)));
        [SerializeField] private SerializableDictionary<Vector2Int, OverlayTileCategory> _overlayTiles;
        public ObservableDictionary<Vector2Int, OverlayTileCategory> OverlayTiles => new(_overlayTiles);

        public TilemapMemento(
            string seed,
            IDictionary<Vector2Int, TileData> tiles,
            IDictionary<Vector2Int, OverlayTileCategory> overlayTiles)
        {
            Seed = seed;
            _tiles = tiles.ToSerializableDictionary(pair => pair.Key, pair => pair.Value.Serialize());
            _overlayTiles = overlayTiles.ToSerializable();
        }

        public TilemapMemento(
            int width,
            TileMemento[] tiles,
            IDictionary<Vector2Int, OverlayTileCategory> overlayTiles)
        {
            Seed = "";
            _tiles = tiles
                .Select((tile, index) => (index, tile))
                .ToSerializableDictionary(pair => new Vector2Int(pair.index % width, pair.index / width), pair => pair.tile);
            _overlayTiles = overlayTiles.ToSerializable();
        }
    }
}