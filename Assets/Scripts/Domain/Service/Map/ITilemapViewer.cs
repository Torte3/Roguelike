using System.Collections.Generic;
using Domain.Model;
using Domain.Model.Map;
using Domain.Model.Memento;
using R3;
using UnityEngine;
using Utilities.Serialize.Option;

namespace Domain.Service.Map
{
    public interface ITilemapViewer : ISerializable<TilemapMemento>
    {
        public Observable<IEnumerable<(Vector2Int Position, TileData Tile)>> OnTilesChanged { get; }
        public Observable<IEnumerable<(Vector2Int Position, TileData Tile)>> OnTilesLoaded { get; }

        public Observable<IEnumerable<(Vector2Int Position, OverlayTileCategory? Category)>> OnOverlayTilesChanged
        {
            get;
        }

        public Observable<IEnumerable<(Vector2Int Position, bool IsKnown)>> OnTilesKnownChanged { get; }
        public ReadOnlyReactiveProperty<RectInt> Rect { get; }
        public bool IsWalkable(Vector2Int position);
        public bool IsPassable(Vector2Int position);
        public bool IsTransparent(Vector2Int position);
        public Option<TileData> GetTile(Vector2Int position);
        public IEnumerable<(Vector2Int position, TileData tileData)> GetAllTiles();
        public IEnumerable<Vector2Int> GetAllGrasses();
        public IEnumerable<Vector2Int> GetAllIces();
        public bool IsGrass(Vector2Int position);
        public bool IsIce(Vector2Int position);
        public HashSet<Vector2Int> GetAllWalkablePositions();
        public HashSet<Vector2Int> GetAllPassablePositions();
        public HashSet<Vector2Int> GetAllLightPassablePositions();
        public void SetTilesKnown(IEnumerable<Vector2Int> positions, bool isKnown);
    }
}