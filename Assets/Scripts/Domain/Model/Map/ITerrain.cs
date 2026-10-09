using UnityEngine;

namespace Domain.Model.Map
{
    public interface ITerrain
    {
        public bool IsWalkable(Vector2Int position);
        public bool IsPassable(Vector2Int position);
        public bool IsTransparent(Vector2Int position);
    }
}
