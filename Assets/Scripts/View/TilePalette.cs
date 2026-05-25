#nullable enable
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace View
{
    public sealed class TilePalette : MonoBehaviour
    {
        [SerializeField] private WorldTiles _worldMapTiles;
        [SerializeField] private Tiles _caveTiles;
        [SerializeField] private Tiles _forestTiles;
        [SerializeField] private Tiles _snowTiles;
        [SerializeField] private Tiles _volcanoTiles;
        [SerializeField] private Tiles _desertTiles;
        [SerializeField] private Tiles _dungeonTiles;
        [SerializeField] private Tiles _voidTiles;
        public (TileBase tile, TileBase? underTile) GetTile(TileSet type, int index)
        {
            if (type == TileSet.WorldMap)
                return index switch
                {
                    1 => (_worldMapTiles.Grass, null),
                    2 => (_worldMapTiles.Ocean, _worldMapTiles.Grass),
                    3 => (_worldMapTiles.Mountain, _worldMapTiles.Grass),
                    4 => (_worldMapTiles.Desert, null),
                    5 => (_worldMapTiles.Forest, _worldMapTiles.Grass),
                    _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
                };
            var tiles = type switch
            {
                TileSet.Cave => _caveTiles,
                TileSet.Forest => _forestTiles,
                TileSet.Snow => _snowTiles,
                TileSet.Volcano => _volcanoTiles,
                TileSet.Desert => _desertTiles,
                TileSet.Dungeon => _dungeonTiles,
                TileSet.Void => _voidTiles,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
            return index switch
            {
                0 => (tiles.Floor, null),
                1 => (tiles.ShopFloor, null),
                2 => (tiles.Water, tiles.Floor),
                3 => (tiles.Wall, null),
                _ => throw new ArgumentOutOfRangeException(nameof(tiles), tiles, null)
            };
        }
    }
}