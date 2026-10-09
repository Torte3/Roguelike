using UnityEngine;
using UnityEngine.Tilemaps;

namespace View
{
    public sealed class MinimapController : MonoBehaviour
    {
        [SerializeField] private Tilemap _tilemap;
        [SerializeField] private Tiles _minimapTiles;

        public void Clear()
        {
            _tilemap.ClearAllTiles();
        }

        public void SetFloor(Vector2Int position)
        {
            SetTile(position, _minimapTiles.Floor);
        }

        public void SetWater(Vector2Int position)
        {
            SetTile(position, _minimapTiles.Water);
        }

        public void SetWall(Vector2Int position)
        {
            SetTile(position, _minimapTiles.Wall);
        }

        public void SetUnbreakableWall(Vector2Int position)
        {
            SetTile(position, _minimapTiles.Wall);
        }

        private void SetTile(Vector2Int position, TileBase tile)
        {
            var color = GetTileColor(position);
            _tilemap.SetTile(new Vector3Int(position.x, position.y, 0), tile);
            SetTileColor(position, color);
        }

        private Color GetTileColor(Vector2Int position)
        {
            var color = _tilemap.GetColor(new Vector3Int(position.x, position.y, 0));
            return color;
        }

        public void SetTileVisibility(Vector2Int position, TileVisibility visibility)
        {
            SetTileColor(position, visibility.GetMinimapColor());
        }

        private void SetTileColor(Vector2Int position, Color color)
        {
            _tilemap.SetTileFlags(new Vector3Int(position.x, position.y, 0), TileFlags.None);
            _tilemap.SetColor(new Vector3Int(position.x, position.y, 0), color);
        }
    }
}