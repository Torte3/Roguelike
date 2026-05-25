using UnityEngine;
using UnityEngine.Tilemaps;

namespace View
{
    public sealed class TileViewController : MonoBehaviour
    {
        [SerializeField] private Tilemap _tilemap;

        public void Clear()
        {
            _tilemap.ClearAllTiles();
        }

        public void SetTile(Vector2Int position, TileBase tile, TileVisibility? visibility = null,
            TileBase underTile = null)
        {
            var color = visibility?.GetColor() ?? GetTileColor(position);
            _tilemap.SetTile(new Vector3Int(position.x, position.y, 0), tile);
            _tilemap.SetTile(new Vector3Int(position.x, position.y, -1), underTile);
            SetTileColor(position, color);
        }

        public Color GetTileColor(Vector2Int position)
        {
            var color = _tilemap.GetColor(new Vector3Int(position.x, position.y, 0));
            return color;
        }

        public void SetTileVisibility(Vector2Int position, TileVisibility visibility)
        {
            SetTileColor(position, visibility.GetColor());
        }

        private void SetTileColor(Vector2Int position, Color color)
        {
            _tilemap.SetTileFlags(new Vector3Int(position.x, position.y, 0), TileFlags.None);
            _tilemap.SetColor(new Vector3Int(position.x, position.y, 0), color);
            _tilemap.SetTileFlags(new Vector3Int(position.x, position.y, -1), TileFlags.None);
            _tilemap.SetColor(new Vector3Int(position.x, position.y, -1), color);
        }
    }
}