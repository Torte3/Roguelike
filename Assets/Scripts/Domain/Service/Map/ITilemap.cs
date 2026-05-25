using System.Collections.Generic;
using Domain.Model.Map;
using UnityEngine;

namespace Domain.Service.Map
{
    public interface ITilemap : ITilemapViewer
    {
        public bool IsPositionInsideMap(Vector2Int position);
        public bool IsPositionInsideActiveChunk(Vector2Int position);
        public void UpdateTurn();
        public void UpdateChunk(Vector2Int position);
        public void RemoveWalls(IEnumerable<Vector2Int> positions);
        public void SetOverlayTiles(IEnumerable<Vector2Int> positions, OverlayTileCategory? category);
    }
}