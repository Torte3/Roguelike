#nullable enable
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Service.Map;
using Unity.Logging;
using UnityEngine;

namespace Game
{
    public class MapPosition : IMapPosition
    {
        public Vector2Int Position { get; init; }
        private IMap _map;
        private ITilemapViewer _tilemapViewer;

        public MapPosition(Vector2Int position, IMap map, ITilemapViewer tilemapViewer)
        {
            Position = position;
            _map = map;
            _tilemapViewer = tilemapViewer;
        }

        public bool IsBlankIgnoreWall(params EntityLayer[] layers)
        {
            if (layers.Any())
                return !_map.GetEntitiesFastAt(Position, layers).Any();
            return !_map.GetEntitiesFastAt(Position).Any();
        }

        public bool IsBlank(params EntityLayer[] layers)
        {
            return IsPassableOnMap() && IsBlankIgnoreWall(layers);
        }

        public bool IsBlankAndStandable(params EntityLayer[] layers)
        {
            return IsWalkableOnMap() && IsBlankIgnoreWall(layers);
        }

        public bool CanPlace(bool isFlying, bool canThroughWalls, bool ignoreEntity,
            params EntityLayer[] layers)
        {
            if (!layers.Any())
                Log.Warning("No layers specified for CanPlace");

            return (ignoreEntity, canThroughWalls, isFlying) switch
            {
                (true, true, _) => _map.IsInside(Position),
                (true, false, true) => IsPassableOnMap(),
                (true, false, false) => IsWalkableOnMap(),
                (false, true, _) => _map.IsInside(Position) && IsBlankIgnoreWall(layers),
                (false, false, true) => IsBlank(layers),
                (false, false, false) => IsBlankAndStandable(layers)
            };
        }

        public bool IsWalkable(IAffiliation actor)
        {
            if (!IsWalkableOnMap())
                return false;
            var entity = _map.GetEntityFastAt(Position, EntityLayer.Middle);
            if (entity == null)
                return true;
            if (entity is ICharacter character && !character.IsPlayer)
                return !character.Affiliation.IsEnemy(actor);
            return false;
        }

        public bool IsWalkableOnMap()
        {
            return _tilemapViewer.IsWalkable(Position);
        }

        public bool IsPassableOnMap()
        {
            return _tilemapViewer.IsPassable(Position);
        }

        public bool IsLightPassable()
        {
            return _tilemapViewer.IsTransparent(Position);
        }
    }
}