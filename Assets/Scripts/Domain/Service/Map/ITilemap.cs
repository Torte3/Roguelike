using System.Collections.Generic;
using Domain.Model;
using Domain.Model.Map;
using Domain.Model.Memento;
using R3;
using UnityEngine;

namespace Domain.Service.Map
{
    public interface ITilemap : ISerializable<TilemapMemento>, ITerrain
    {
        public Observable<IEnumerable<(Vector2Int Position, TileData Tile)>> OnTilesChanged { get; }

        public Observable<IEnumerable<(Vector2Int Position, OverlayTileCategory? Category)>> OnOverlayTilesChanged
        {
            get;
        }

        public RectInt Rect { get; }
        public IEnumerable<(Vector2Int position, TileData tileData)> GetAllTiles();
        public IEnumerable<(Vector2Int Position, OverlayTileCategory Category)> GetAllOverlayTiles();
        public bool IsGrass(Vector2Int position);
        public HashSet<Vector2Int> GetAllWalkablePositions();
        public HashSet<Vector2Int> GetAllPassablePositions();
        public HashSet<Vector2Int> GetAllLightPassablePositions();
        public void SetTilesKnown(IEnumerable<Vector2Int> positions, bool isKnown);
        public bool IsPositionInsideMap(Vector2Int position);
        public void UpdateTurn();
        public IReadOnlyList<(Vector2Int Position, TileData PreviousTile)> RemoveWalls(IEnumerable<Vector2Int> positions);
        public void SetOverlayTiles(IEnumerable<Vector2Int> positions, OverlayTileCategory? category);
    }
}
