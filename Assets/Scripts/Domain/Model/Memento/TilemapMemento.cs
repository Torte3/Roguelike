#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Domain.Model.Map;
using ObservableCollections;
using UnityEngine;
using Utilities.Serialize;

namespace Domain.Model.Memento
{
    [Serializable]
    public class TilemapMemento
    {
        private const char NoTile = ' ';
        private const char FirstCode = '!';
        private const int IndexCount = 8;

        [SerializeField] private Vector2Int _tileOrigin;
        [SerializeField] private int _tileWidth;
        [SerializeField] private string _tileCodes = "";
        [SerializeField] private SerializableDictionary<Vector2Int, OverlayTileCategory> _overlayTiles;

        public ObservableDictionary<Vector2Int, TileData> Tiles =>
            new(DecodeTiles().ToDictionary(pair => pair.Key, pair => new TileData(pair.Value)));
        public ObservableDictionary<Vector2Int, OverlayTileCategory> OverlayTiles => new(_overlayTiles);

        public TilemapMemento(
            IDictionary<Vector2Int, TileData> tiles,
            IDictionary<Vector2Int, OverlayTileCategory> overlayTiles)
        {
            EncodeTiles(tiles.ToDictionary(pair => pair.Key, pair => pair.Value.Serialize()));
            _overlayTiles = overlayTiles.ToSerializable();
        }

        public TilemapMemento(
            int width,
            TileMemento[] tiles,
            IDictionary<Vector2Int, OverlayTileCategory> overlayTiles)
        {
            EncodeTiles(tiles
                .Select((tile, index) => (index, tile))
                .ToDictionary(pair => new Vector2Int(pair.index % width, pair.index / width), pair => pair.tile));
            _overlayTiles = overlayTiles.ToSerializable();
        }

        private void EncodeTiles(IReadOnlyDictionary<Vector2Int, TileMemento> tiles)
        {
            if (tiles.Count == 0)
                return;

            var min = new Vector2Int(tiles.Keys.Min(position => position.x), tiles.Keys.Min(position => position.y));
            var max = new Vector2Int(tiles.Keys.Max(position => position.x), tiles.Keys.Max(position => position.y));
            _tileOrigin = min;
            _tileWidth = max.x - min.x + 1;
            var codes = new StringBuilder(_tileWidth * (max.y - min.y + 1));
            for (var y = min.y; y <= max.y; y++)
            {
                for (var x = min.x; x <= max.x; x++)
                {
                    codes.Append(tiles.TryGetValue(new Vector2Int(x, y), out var tile) ? ToCode(tile) : NoTile);
                }
            }

            _tileCodes = codes.ToString();
        }

        private IEnumerable<KeyValuePair<Vector2Int, TileMemento>> DecodeTiles()
        {
            return _tileCodes
                .Select((code, index) => (code, index))
                .Where(pair => pair.code != NoTile)
                .Select(pair => new KeyValuePair<Vector2Int, TileMemento>(
                    _tileOrigin + new Vector2Int(pair.index % _tileWidth, pair.index / _tileWidth),
                    FromCode(pair.code)));
        }

        private static char ToCode(TileMemento tile)
        {
            if (tile.Index is < 0 or >= IndexCount)
                throw new ArgumentOutOfRangeException(nameof(tile), tile.Index, null);
            return (char)(FirstCode + (((int)tile.MapType * IndexCount + tile.Index) << 1) + (tile.IsKnown ? 1 : 0));
        }

        private static TileMemento FromCode(char code)
        {
            var value = code - FirstCode;
            var typeAndIndex = value >> 1;
            return new TileMemento((MapType)(typeAndIndex / IndexCount), typeAndIndex % IndexCount, (value & 1) == 1);
        }
    }
}
